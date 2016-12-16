using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

using Mono.Reflection;

namespace FindCrossRefs
{
    class Program
    {

        private static Dictionary<string, string> functionIndex;
        private static string dbName;
        private static DBSet db;

        static void Main(string[] args)
        {
            string typeToCheck = args[0];
            Console.WriteLine("Process started");
            List<string> assemblyList = ReadList(args[0]);
            db = new DBSet(dbName);

            ReadAssemblies(assemblyList);

            db.Dispose();
            //Type type         = assembly.GetType(args[1]); ;
            //Check(type, args[2]);
            Console.WriteLine("Process finished. Press [Enter] to continue...");
            Console.ReadLine();
        }

        static void Check(Type checkType, string methodName)
        {
            
            MethodBase methodBase = checkType.GetMethod(methodName, new Type[] { typeof(Int32)} );
            var instructions = MethodBodyReader.GetInstructions(methodBase);

            foreach (Instruction instruction in instructions)
            {
                MethodInfo methodInfo = instruction.Operand as MethodInfo;

                if (methodInfo != null)
                {
                    Type type = methodInfo.DeclaringType;
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    Console.WriteLine("{0}.{1}({2});",
                        type.FullName,
                        methodInfo.Name,
                        String.Join(", ", parameters.Select(p => p.ParameterType.FullName + " " + p.Name).ToArray())
                    );
                }
            }
        }

        static List<string> ReadList(string scriptName)
        {
            List<string> list = new List<string>();

            List<string> lines =  File.ReadLines(scriptName).ToList();

            lines.ForEach(l => {
                if (l.StartsWith("in:"))
                {
                    list.Add(l.Substring(3));
                }
                else if (l.StartsWith("out:"))
                {
                    dbName = l.Substring(4);
                }
            });

            return list;
        }

        static void ReadAssemblies(List<string> assemblyList)
        {
            assemblyList.ForEach(a =>
            {
                Console.WriteLine("Reading {0}", a);
                Assembly assembly = Assembly.LoadFile(Path.Combine(Environment.CurrentDirectory, a));
                List<Type> types  =  assembly.GetTypes().ToList();
                types.ForEach(t => {
                    List<MethodInfo> methods = t.GetMethods().ToList();
                    methods.ForEach(m => {
                        MethodBase methodBase = m;

                        ParameterInfo[] parameters = m.GetParameters();
                        string functionName = string.Format("{0}({1})",
                            m.Name,
                            String.Join(", ", parameters.Select(p => p.ParameterType.FullName).ToArray())
                        );
                        int functId = db.WriteFunction( t.Namespace, t.Name, functionName);
                        
                        var instructions = MethodBodyReader.GetInstructions(methodBase);

                        foreach (Instruction instruction in instructions)
                        {
                            MethodInfo methodInfo = instruction.Operand as MethodInfo;

                            if (methodInfo != null)
                            {
                                Type type = methodInfo.DeclaringType;
                                ParameterInfo[]  instParameters = methodInfo.GetParameters();

                                string instName = string.Format( "{0}({1})",
                                    methodInfo.Name,
                                    String.Join(", ", instParameters.Select(p => p.ParameterType.FullName ).ToArray())
                                );

                                db.WriteReference(functId, type.Namespace, type.Name, instName);
                            }
                        }
                    });
                });
            });
        }
    }
}
