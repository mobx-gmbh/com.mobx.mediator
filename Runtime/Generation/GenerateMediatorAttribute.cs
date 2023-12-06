using System;
using System.Runtime.CompilerServices;

namespace MobX.Mediator.Generation
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class GenerateMediatorAttribute : Attribute
    {
        public MediatorTypes MediatorTypes { get; }

        public string FilePath { get; }

        /// <summary>
        ///     Optional namespace override for the generated mediator file.
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        ///     Optional subfolder.
        /// </summary>
        public string Subfolder { get; set; }

        public GenerateMediatorAttribute(MediatorTypes mediatorTypes, [CallerFilePath] string filePath = default)
        {
            MediatorTypes = mediatorTypes;
            FilePath = filePath;
        }

        public GenerateMediatorAttribute([CallerFilePath] string filePath = default)
        {
            MediatorTypes = MediatorTypes.Everything;
            FilePath = filePath;
        }
    }
}