using System;
using System.Resources;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace News.Core.Services.Web
{
    public static class ResourceManagerExtensions
    {
        /// <summary>
        /// Resource manager extensions
        /// </summary>
        public static MemoryStream GetMemoryStream(this ResourceManager resourceManager, String name)
        {
            object resource = resourceManager.GetObject(name);

            if (resource is byte[])
            {
                return new MemoryStream((byte[])resource);
            }
            else
            {
                throw new System.InvalidCastException("The specified resource is not a binary resource.");
            }
        }
    }
}
