
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("JTran.UnitTests")]

namespace JTran.Common
{
    internal interface IObject 
    {
        object? GetPropertyValue(ICharacterSpan name);
    }
}
