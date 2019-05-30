using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

public class ConsoleInputer : SingletonBehaviour<ConsoleInputer>
{
    [Header("Enable / Disable")]
    [SerializeField] private GameObject consoleParent = null;
    //[SerializeField] private SelectedBlockVisualizer selectedBockVisualizer;

    [SerializeField] private IConsoleToggle[] disableOnConsoleAppear = null;


    [Header("UI")]
    [SerializeField] private TMP_InputField inputfield = null;
    [SerializeField] private TextMeshProUGUI consoleOutput = null;

    private Dictionary<string, MethodInfo> dictionary;

    private bool showingConsole = false;

    private void Start()
    {
        dictionary = new Dictionary<string, MethodInfo>();

        var methods = Assembly.GetAssembly(typeof(ConsoleInputer)).GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            .Where(m => m.GetCustomAttributes(typeof(ConsoleMethodAttribute), false).Length > 0)
            .ToArray();

        for (int i = 0; i < methods.Length; i++)
        {
            dictionary.Add(methods[i].GetCustomAttribute<ConsoleMethodAttribute>().stringName.ToLower(), methods[i]);
        }

        //Welche Consolenmethoden wurden erkannt?
        //foreach (KeyValuePair<string, MethodInfo> entry in dictionary)
        //{
        //    Debug.Log(entry.Key);
        //}

        disableOnConsoleAppear = FindObjectsOfType<MonoBehaviour>().OfType<IConsoleToggle>().ToArray();

        var s = new TMP_InputField.SubmitEvent();
        inputfield.onSubmit = s;

        s.AddListener((string message) =>
        {

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
                    return;
                }


                para = new object[substrings.Length - 1];
                for (int i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        para[i] = Convert.ChangeType(substrings[i + 1], parameters[i].ParameterType);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
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

                        if (method.GetParameters().Length > 0)
                        {
                            method.Invoke(objects[i], para);
                        }
                        else
                        {
                            method.Invoke(objects[i], null);
                        }
                    }
                }
            }
            else
            {
                consoleOutput.text += message + " has not been found\n";
            }

            inputfield.text = "";
            consoleOutput.text += message + "\n";
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showingConsole = !showingConsole;

            foreach (Transform child in consoleParent.transform)
            {
                child.gameObject.SetActive(showingConsole);
            }

            foreach (IConsoleToggle toggleObject in disableOnConsoleAppear)
            {
                toggleObject.Enabled = !showingConsole;
            }
        }
    }
}