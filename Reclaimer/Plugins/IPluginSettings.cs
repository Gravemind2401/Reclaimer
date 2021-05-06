using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Plugins
{
    public interface IPluginSettings
    {
        //if a property is missing during json deserialisation, it will be left
        //as the default value of its type. this is fine for primitives and allows
        //you to set a default value in the constructor that will be overwritten if
        //present in the json or otherwise left as-is. for reference types
        //(or at least collections) if the constructor has already instanciated the
        //property then the deserialiser seems to use the existing object rather
        //than creating a new one - this results in lists being duplicated.
        //this function provides a way to only set a reference value if it
        //was not instanciated by the deserialiser (not present in json)
        void ApplyDefaultValues(bool newInstance);
    }
}
