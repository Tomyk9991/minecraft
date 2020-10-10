using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Core.Managers;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI.Console
{
    public class ConsoleInputer : SingletonBehaviour<ConsoleInputer>, IFullScreenUIToggle
    {
        [Header("Enable / Disable")] 
        [SerializeField] private Transform[] consoleToggleTransforms = null;

        [SerializeField] private IConsoleToggle[] disableOnConsoleAppear = null;

        [Header("UI")] [SerializeField] private TMP_InputField inputfield = null;
        [SerializeField] private TextMeshProUGUI consoleOutput = null;
        [SerializeField] private TMP_Text previewText = null;
        
        //private Dictionary<string, MethodInfo> dictionary;
        private Dictionary<string, ReflectionMethodInfo> dictionary;
        private string currentMessage = "";
        private int tabCounter = 0;
        private int inputHistoryClickCounter = 0;
        private int lineCounter = 0;
        private bool showingConsole = false;

        private string inputHistory;
        
        public bool Enabled
        {
            get => this.enabled;
            set => this.enabled = value;
        }
        
        private void Start()
        {
            //dictionary = new Dictionary<string, MethodInfo>();
            dictionary = new Dictionary<string, ReflectionMethodInfo>();
            inputHistory = "";
            
            MethodInfo[] methods = Assembly.GetAssembly(typeof(ConsoleInputer)).GetTypes()
                .SelectMany((Type t) =>
                    t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                 BindingFlags.Static))
                .Where((MethodInfo m) => m.GetCustomAttributes(typeof(ConsoleMethodAttribute), false).Length > 0)
                .ToArray();

            for (int i = 0; i < methods.Length; i++)
            {
                ConsoleMethodAttribute currentAttribute = methods[i].GetCustomAttribute<ConsoleMethodAttribute>();
                
                string methodName = currentAttribute.stringName;
                string methodDescription = currentAttribute.description ?? "";

                if (!dictionary.ContainsKey(methodName))
                {
                    dictionary.Add(methodName, new ReflectionMethodInfo(methods[i], methodDescription));

                    string toLowerMethodName = methodName.ToLower();

                    if (toLowerMethodName != methodName)
                        dictionary.Add(toLowerMethodName, new ReflectionMethodInfo(methods[i], methodDescription));
                }
            }

            // Welche Consolenmethoden wurden erkannt?
            // DEBUGGING: Wird nicht genutzt
            StringBuilder builder = new StringBuilder("{");
            int index = 0;
            foreach (KeyValuePair<string, ReflectionMethodInfo> entry in dictionary)
            {
                builder.Append(entry.Key);
                builder.Append(index == dictionary.Count - 1 ? "" : ", ");
                index += 1;
            }

            builder.Append("}");

            disableOnConsoleAppear = FindObjectsOfType<MonoBehaviour>().OfType<IConsoleToggle>().ToArray();

            var s0 = new TMP_InputField.SubmitEvent();
            inputfield.onSubmit = s0;
            s0.AddListener(ProcessMessage);
            OnValueChanged("");
        }

        //Called from Unity
        public void OnValueChanged(string message)
        {
            currentMessage = message;

            if(!String.IsNullOrEmpty(currentMessage))
            {
                string currentMessageWithoutStringsAfterFirstSpace = currentMessage.Split(' ')[0];
                var tuple = ClosestString(currentMessageWithoutStringsAfterFirstSpace, dictionary.Keys.ToArray());
                string closestMessage = tuple.searchResult;


                if (tuple.hasFound)
                {
                    ReflectionMethodInfo info = dictionary[closestMessage];
                    
                    previewText.gameObject.SetActive(true);
                    string parameterString = ParameterString(info.Info.GetParameters());
                    previewText.text = closestMessage + parameterString + " " + RichTextCodedDescription(info.Description);
                }
                else
                {
                    previewText.gameObject.SetActive(false);
                }
            }
            else
            {
                previewText.gameObject.SetActive(false);
            }
        }

        private void ProcessMessage(string message)
        {
            inputHistory = message;
            inputHistoryClickCounter = 0;
            if (String.IsNullOrEmpty(message) || inputfield.wasCanceled)
            {
                EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
                inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
                previewText.gameObject.SetActive(false);
                return;
            }

            currentMessage.Trim();
            if (currentMessage.StartsWith("="))
            {
                try
                {
                    currentMessage = currentMessage.TrimStart('=');
                    var loDataTable = new DataTable();
                    var loDataColumn = new DataColumn("Eval", typeof (double), currentMessage); 
                    loDataTable.Columns.Add(loDataColumn);
                    loDataTable.Rows.Add(0); 
                    double d = (double) loDataTable.Rows[0]["Eval"];
                    previewText.gameObject.SetActive(true);
                    Write(currentMessage + " = " + d);
                }
                catch (Exception e)
                {
                    WriteToOutput(e.Message);
                }
                
                inputfield.text = "";
                EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
                inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
                return;
            }

            string[] substrings = message.Split(' ');
            message = substrings[0];
            object[] para;

            if (dictionary.TryGetValue(message, out ReflectionMethodInfo method))
            {
                var parameters = method.Info.GetParameters();

                if (substrings.Length - 1 != parameters.Length)
                {
                    Write(message + " Parameters not correct");
                    inputfield.text = "";
                    EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
                    inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
                    return;
                }


                para = new object[substrings.Length - 1];
                bool wrongParas = false;
                for (int i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        para[i] = Convert.ChangeType(substrings[i + 1], parameters[i].ParameterType);
                    }
                    catch (Exception)
                    {
                        wrongParas = true;
                    }
                }

                if (method.Info.IsStatic)
                {
                    method.Info.Invoke(null, null);
                }
                else
                {
                    var objects = FindObjectsOfType(method.Info.ReflectedType);

                    for (int i = 0; i < objects.Length; i++)
                    {
                        if (!wrongParas)
                            method.Info.Invoke(objects[i], method.Info.GetParameters().Length > 0 ? para : null);
                    }
                }
            }
            else
            {
                Write(message + " has not been found");
                inputfield.text = "";
                EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
                inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
                return;
            }

            if (message.ToLower() != "clear")
                Write(inputfield.text);

            inputfield.text = "";

            EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
            inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        private (string searchResult, bool hasFound) ClosestString(string cmpString, string[] strings)
        {
            string cmptmpLwr = cmpString.ToLower();
            foreach (var t in strings)
            {
                string tmpLwr = t.ToLower();
                bool eval = tmpLwr.StartsWith(cmptmpLwr) && !string.IsNullOrEmpty(cmptmpLwr);

                if (eval)
                {
                    tabCounter = 0;
                    return (t, true);
                }
            }

            int idx = tabCounter % strings.Length;
            string s = strings[idx];
            tabCounter++;
            return (s, false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
                ToggleConsole();

            if (showingConsole && Input.GetKeyDown(KeyCode.Tab))
                CalculateClosestString();

            if (showingConsole && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)) && inputHistory != "")
                ToggleInputHistory();
        }

        private void ToggleInputHistory()
        {
            inputfield.text = inputHistoryClickCounter % 2 == 0 ? inputHistory : "";
            inputHistoryClickCounter += 1;
            inputfield.caretPosition = inputHistory.Length;
        }

        private void CalculateClosestString()
        {
            string closestMessage = ClosestString(currentMessage, dictionary.Keys.ToArray()).searchResult;
            string tempCurMessage = currentMessage;
            inputfield.text = closestMessage;

            currentMessage = tempCurMessage;
            EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
            inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
            inputfield.caretPosition = closestMessage.Length;
        }

        private void ToggleConsole()
        {
            showingConsole = !showingConsole;
            CursorVisibilityManager.Instance.ToggleMouseVisibility();

            foreach (Transform child in consoleToggleTransforms)
            {
                child.gameObject.SetActive(showingConsole);
            }

            foreach (IConsoleToggle toggleObject in disableOnConsoleAppear)
            {
                toggleObject.Enabled = !showingConsole;
            }

            EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
            inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        private string ParameterString(ParameterInfo[] infos)
        {
            StringBuilder builder = new StringBuilder("{");

            for (int i = 0; i < infos.Length; i++)
            {
                builder
                    .Append(infos[i].ParameterType.Name)
                    .Append(" ")
                    .Append(infos[i].Name)
                    .Append((i + 1) != infos.Length
                        ? ", "
                        : ""
                    );
            }

            builder.Append("}");

            return builder.ToString();
        }
        
        private string RichTextCodedDescription(string infoDescription) 
            => "<b><i><size=12><color=#ffffffaa>" + infoDescription + "</color></size></i></b>";


        public void Write(string buffer)
        {
            lineCounter++;
            if (lineCounter >= 16)
            {
                string currentOutputString = consoleOutput.text;
                int index = currentOutputString.IndexOf('\n');
                consoleOutput.text = currentOutputString.Remove(0, index + 1);
            }
            consoleOutput.text += buffer + "\n";
        }

        public static void WriteToOutput(string buffer) => Instance.Write(buffer);

        [ConsoleMethod(nameof(Clear), "Clears the console")]
        private void Clear()
        {
            consoleOutput.text = "";
            inputfield.text = "";
            currentMessage = "";
            EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
            inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
            lineCounter = 0;
        }
        
        private class ReflectionMethodInfo
        {
            public MethodInfo Info;
            public string Description;
            
            public ReflectionMethodInfo(MethodInfo info, string description)
            {
                Info = info;
                Description = description;
            }
        }

    }
}
