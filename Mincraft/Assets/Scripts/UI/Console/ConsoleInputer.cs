using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Core.Managers;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Core.UI.Console
{
    public class ConsoleInputer : SingletonBehaviour<ConsoleInputer>
    {
        [Header("Enable / Disable")] [SerializeField]
        private GameObject consoleParent = null;

        [SerializeField] private IConsoleToggle[] disableOnConsoleAppear = null;

        [Header("UI")] [SerializeField] private TMP_InputField inputfield = null;
        [SerializeField] private TextMeshProUGUI consoleOutput = null;
        [SerializeField] private TMP_Text previewText = null;


        private Dictionary<string, MethodInfo> dictionary;
        private string currentMessage = "";
        private int tabCounter = 0;
        private bool showingConsole = false;

        private void Start()
        {
            dictionary = new Dictionary<string, MethodInfo>();

            var methods = Assembly.GetAssembly(typeof(ConsoleInputer)).GetTypes()
                .SelectMany(t =>
                    t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance |
                                 BindingFlags.Static))
                .Where(m => m.GetCustomAttributes(typeof(ConsoleMethodAttribute), false).Length > 0)
                .ToArray();

            for (int i = 0; i < methods.Length; i++)
            {
                string methodName = methods[i].GetCustomAttribute<ConsoleMethodAttribute>().stringName;

                if (!dictionary.ContainsKey(methodName))
                {
                    dictionary.Add(methodName, methods[i]);

                    string toLowerMethodName = methodName.ToLower();

                    if (toLowerMethodName != methodName)
                        dictionary.Add(toLowerMethodName, methods[i]);
                }
            }

            //Welche Consolenmethoden wurden erkannt?
            StringBuilder builder = new StringBuilder("{");
            int index = 0;
            foreach (KeyValuePair<string, MethodInfo> entry in dictionary)
            {
                builder.Append(entry.Key);
                builder.Append(index == dictionary.Count - 1 ? "" : ", ");
                index += 1;
            }

            builder.Append("}");

            disableOnConsoleAppear = FindObjectsOfType<MonoBehaviour>().OfType<IConsoleToggle>().ToArray();

            var s = new TMP_InputField.SubmitEvent();
            inputfield.onSubmit = s;

            s.AddListener(ProcessMessage);
        }

        //Called from Unity
        public void OnValueChanged(string message)
        {
            currentMessage = message;
        }

        private void ProcessMessage(string message)
        {
            if (String.IsNullOrEmpty(message))
            {
                EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
                inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
                previewText.gameObject.SetActive(false);
                return;
            }
            
            if (currentMessage.Contains("+") || currentMessage.Contains("-") || currentMessage.Contains("*") ||
                currentMessage.Contains("/"))
            {
                try
                {
                    var loDataTable = new DataTable(); 
                    var loDataColumn = new DataColumn("Eval", typeof (double), currentMessage); 
                    loDataTable.Columns.Add(loDataColumn); 
                    loDataTable.Rows.Add(0); 
                    double d = (double) loDataTable.Rows[0]["Eval"];
                    previewText.gameObject.SetActive(true);
                    consoleOutput.text = currentMessage + " = " + d;
                }
                catch (SyntaxErrorException e)
                {
                    
                }
                
                inputfield.text = "";
                EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
                inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
                return;
            }

            string[] substrings = message.Split(' ');
            message = substrings[0];
            object[] para;

            if (dictionary.TryGetValue(message, out MethodInfo method))
            {
                var parameters = method.GetParameters();

                if (substrings.Length - 1 != parameters.Length)
                {
                    consoleOutput.text += message + " Parameters not correct\n";
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
                    catch (Exception e)
                    {
                        wrongParas = true;
                    }
                }

                if (method.IsStatic)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    var objects = FindObjectsOfType(method.ReflectedType);

                    for (int i = 0; i < objects.Length; i++)
                    {
                        if (!wrongParas)
                            method.Invoke(objects[i], method.GetParameters().Length > 0 ? para : null);
                    }
                }
            }
            else
            {
                consoleOutput.text += message + " has not been found\n";
                inputfield.text = "";
                EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
                inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
                return;
            }

            inputfield.text = "";
            if (message.ToLower() != "clear")
            {
                consoleOutput.text += message + "\n";
            }

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
            {
                showingConsole = !showingConsole;
                CursorVisibilityManager.Instance.ToggleMouseVisibility();

                foreach (Transform child in consoleParent.transform)
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

            if (showingConsole && !String.IsNullOrEmpty(currentMessage))
            {
                string currentMessageWithoutStringsAfterFirstSpace = currentMessage.Split(' ')[0];
                var tuple = ClosestString(currentMessageWithoutStringsAfterFirstSpace, dictionary.Keys.ToArray());
                string closestMessage = tuple.searchResult;

                
                if (tuple.hasFound)
                {
                    previewText.gameObject.SetActive(true);
                    string parameterString = ParameterString(dictionary[closestMessage].GetParameters());
                    previewText.text = closestMessage + parameterString;
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

            if (showingConsole && Input.GetKeyDown(KeyCode.Tab))
            {
                string closestMessage = ClosestString(currentMessage, dictionary.Keys.ToArray()).searchResult;
                string tempCurMessage = currentMessage;
                inputfield.text = closestMessage;

                currentMessage = tempCurMessage;
                EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
                inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
                inputfield.caretPosition = closestMessage.Length;
            }
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

        [ConsoleMethod(nameof(Clear))]
        private void Clear()
        {
            consoleOutput.text = "";
            inputfield.text = "";
            currentMessage = "";
            EventSystem.current.SetSelectedGameObject(inputfield.gameObject, null);
            inputfield.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        public static void Write(string buffer)
        {
            Instance.consoleOutput.text += buffer + "\n";
        }
    }
}