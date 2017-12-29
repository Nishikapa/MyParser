using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace MyParser
{
    static partial class Program
    {
        const string AssemblyName = "Test";
        const string AssemblyExeName = AssemblyName + ".exe";

        static void 式木からExeの生成(this Expression<Action> lambda)
        {
            var appDomain = AppDomain.CurrentDomain;

            var assemblyBuildser = appDomain.DefineDynamicAssembly(
                new AssemblyName(AssemblyName),
                AssemblyBuilderAccess.Save
            );

            var moduleBuilder =
                assemblyBuildser.DefineDynamicModule(AssemblyName, AssemblyExeName);

            var typeBuilder =
                moduleBuilder.DefineType(
                    "Program",
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.Sealed 
                );

            var methodBuilder =
                typeBuilder.DefineMethod(
                    "Main",
                    MethodAttributes.Static | MethodAttributes.Public
                    );

            lambda.CompileToMethod(methodBuilder);

            typeBuilder.CreateType();

            assemblyBuildser.SetEntryPoint(methodBuilder, PEFileKinds.ConsoleApplication);

            assemblyBuildser.Save(AssemblyExeName);
        }
    }
}
