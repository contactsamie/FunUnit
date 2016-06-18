using Microsoft.CSharp;

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FunUnit
{
    public class DynaInvoke
    {
        private IFunUnitNotification Notification
        {
            set;
            get;
        }

        public DynaInvoke(IFunUnitNotification notification)
        {
            Notification = notification;
        }

        public List<Tuple<bool,
        string>> InvokeMethod(string assemblyName, object[] args)
        {
            var assembly = Assembly.LoadFrom(assemblyName);
            return InvokeMethod(assembly, args);
        }

        public List<Tuple<bool,
        string>> InvokeMethod(Assembly assembly, object[] args)
        {
            var results = new List<Tuple<bool,
            string>>();

            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsClass)
                    continue;
                var classObj = Activator.CreateInstance(type);

                foreach (
                    var methodInfo in
                    type.GetMethods()
                    .Where(methodInfo => methodInfo.GetCustomAttributes(typeof(TestMethodAttribute), false).Length > 0))
                {
                    var method = type.GetMethod(methodInfo.Name);
                    try
                    {
                        method.Invoke(classObj, null); //, args);

                        var pass = type.Name + " Then " + methodInfo.Name;
                        results.Add(new Tuple<bool, string>(true, pass));
                    }
                    catch (Exception e)
                    {
                        var fail = type.Name + " Then " + methodInfo.Name + " > " + e.InnerException?.Message;
                        results.Add(new Tuple<bool, string>(false, fail));
                    }
                }
            }
            return results;
        }

        public Tuple<StringCollection,Assembly> Compile(string[] dllFiles, string[] sourceFiles, string outputAssemblyPath)
        {
            var providerOptions = new Dictionary<string,
            string> { {
                    "CompilerVersion",
                    "v4.0"
                }
            };
            CodeDomProvider codeProvider = new CSharpCodeProvider(providerOptions);
            var compilerParameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = true
            };

            compilerParameters.ReferencedAssemblies.AddRange(dllFiles);
            compilerParameters.ReferencedAssemblies.Add("System.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Core.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Web.Services.dll");
            compilerParameters.ReferencedAssemblies.Add("System.ComponentModel.dll");
            compilerParameters.ReferencedAssemblies.Add("System.Xml.Serialization.dll");
            var result = codeProvider.CompileAssemblyFromFile(compilerParameters, sourceFiles);
            return new Tuple<StringCollection,
            Assembly>(result.Output, result.Errors.Count > 0 ? null : result.CompiledAssembly);
        }

        public void Watch()
        {
            var path = Notification.GetSourcePath();
            var watcher = new FileSystemWatcher
            {
                Path = path,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = "*.*"
            };
            watcher.Changed += OnChanged;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
        }

        private DateTime _lastChanged = DateTime.Now;

        private void OnChanged(object source, FileSystemEventArgs e)
        {
            var path = ((FileSystemWatcher)source).Path;
            if ((DateTime.Now - _lastChanged).TotalMilliseconds < 1000)
                return;
            _lastChanged = DateTime.Now;
            try
            {
                RetryableExecute(() => {
                    Console.WriteLine("I'm building stuff right now ....");
                    var current = path +  @"\bin\Debug";
                    var parent = Directory.GetParent(current);
                    parent = Directory.GetParent(parent.FullName);
                    var files = Directory.GetFiles(parent.FullName).Where(x => x.EndsWith(".cs")).ToArray();
                    var dllFiles = Directory.GetFiles(current).Where(x => x.EndsWith(".dll") && !x.Contains("UnitTestProject1.dll")).ToArray();
                    var output = Compile(dllFiles, files, current +  @"\me");

                    if (output.Item2 == null)
                    {
                        Notification.OnBuildError("CANNOT BUILD SOURCE - " + path);
                    }
                    else
                    {
                        var result = InvokeMethod(output.Item2, new object[]{
                                    null
                                });

                        foreach (var tuple in result)
                        {
                            if (tuple.Item1)
                            {
                                Notification.OnSuccess("PASSING - " + tuple.Item2);
                            }
                            else
                            {
                                Notification.OnFailure("FAILING - " + tuple.Item2);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Notification.OnGeneralError(ex);
            }
        }

        public void RetryableExecute(Action opeartion, int maxNumberOfRetries = 4, int retryIntervalMilliseconds = 1000)
        {
            var retry = 0;
            var lastException = new Exception();
            while (retry < maxNumberOfRetries)
            {
                retry++;
                try
                {
                    opeartion();
                    break;
                }
                catch (Exception e)
                {
                    lastException = e;
                    System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(retryIntervalMilliseconds));
                }
            }
            throw lastException;
        }
    }

}
