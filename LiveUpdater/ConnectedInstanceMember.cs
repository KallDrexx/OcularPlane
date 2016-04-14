using FlatRedBall.Glue.Parsing;
using FlatRedBall.Glue.Plugins;
using OcularPlane.Models;
using OcularPlane.Networking.WcfTcp.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfDataUi.DataTypes;

namespace LiveUpdater
{
    class ConnectedInstanceMember : InstanceMember
    {
        public InstanceReference InstanceReference { get; set; }
        public PropertyReference PropertyReference { get; set; }

        public OcularPlaneClient Client { get; set; }

        Type type;
        private TypeConverter typeConverter;

        public Type Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;

                if (type != null)
                {
                    this.typeConverter = TypeDescriptor.GetConverter(type);
                }
            }
        }


        public ConnectedInstanceMember()
        {
            this.CustomGetEvent += HandleGet;
            this.CustomSetEvent += HandleSet;
            this.CustomGetTypeEvent += HandleGetType;
        }

        private Type HandleGetType(object owner)
        {
            return Type;
        }

        private void HandleSet(object owner, object value)
        {
            string asString = null;
            if(value != null)
            {
                asString = value.ToString();
            }

            if(Client != null)
            {
                try
                {
                    Client.SetPropertyValue(InstanceReference.InstanceId, PropertyReference.Name, asString);
                    // It might be a little bit before we update from the host again, so if we succeeded, let's
                    // update the value locally:
                    PropertyReference.ValueAsString = asString;
                }
                catch(Exception e)
                {
                    PluginManager.ReceiveError(e.ToString());
                }
            }

        }

        private object HandleGet(object owner)
        {
            if (typeConverter != null)
            {
                try
                {
                    //return Connection.GetProperty(InstanceReference, PropertyReference);
                    var asString = PropertyReference.ValueAsString;
                    return typeConverter.ConvertTo(asString, type);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
