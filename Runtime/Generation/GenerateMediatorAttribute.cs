using System;
using System.Runtime.CompilerServices;

namespace MobX.Mediator.Generation
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class GenerateMediatorAttribute : Attribute
    {
        public MediatorTypes MediatorTypeses { get; }
        public string FilePath { get; }
        public string NameSpace { get; set; }

        public GenerateMediatorAttribute(MediatorTypes mediatorTypeses, [CallerFilePath] string filePath = default)
        {
            MediatorTypeses = mediatorTypeses;
            FilePath = filePath;
        }

        public GenerateMediatorAttribute([CallerFilePath] string filePath = default)
        {
            MediatorTypeses = MediatorTypes.Everything;
            FilePath = filePath;
        }
    }
}