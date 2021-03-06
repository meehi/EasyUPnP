﻿using EasyUPnP.Common;
using EasyUPnP.Utils;
using System;
using System.Threading.Tasks;

namespace EasyUPnP.Device
{
    public class MediaRenderer
    {
        #region Constructors

        public MediaRenderer()
        {
        }

        public MediaRenderer(DeviceDescription deviceDescription, Uri deviceDescriptionUrl)
        {
            this.DeviceDescription = deviceDescription;

            if (string.IsNullOrEmpty(this.DeviceDescription.Device.PresentationURL))
            {
                this.DeviceDescription.Device.PresentationURL = deviceDescriptionUrl.Authority;
                if (this.DeviceDescription.Device.PresentationURL.IndexOf("http://") == -1)
                    this.DeviceDescription.Device.PresentationURL = "http://" + this.DeviceDescription.Device.PresentationURL;
            }
            this.PresentationURL = this.DeviceDescription.Device.PresentationURL + Parser.UseSlash(this.DeviceDescription.Device.PresentationURL);
        }

        #endregion

        #region Public properties

        public string PresentationURL { get; private set; }
        public string DefaultIconUrl { get; private set; }
        public MediaRenderer Self { get; private set; }

        public DeviceDescription DeviceDescription { get; private set; }
        public ConnectionManager ConnectionManager { get; private set; }
        public RenderingControl RenderingControl { get; private set; }
        public AVTransport AVTransport { get; private set; }

        #endregion

        #region Public functions

        public async Task InitAsync()
        {
            foreach (Service serv in this.DeviceDescription.Device.ServiceList)
                switch (serv.ServiceType)
                {
                    case ServiceTypes.RENDERINGCONTROL:
                        this.RenderingControl = await Deserializer.DeserializeXmlAsync<RenderingControl>(new Uri(this.PresentationURL + serv.SCPDURL.Substring(1)));
                        break;
                    case ServiceTypes.CONNECTIONMANAGER:
                        this.ConnectionManager = await Deserializer.DeserializeXmlAsync<ConnectionManager>(new Uri(this.PresentationURL + serv.SCPDURL.Substring(1)));
                        break;
                    case ServiceTypes.AVTRANSPORT:
                        this.AVTransport = await Deserializer.DeserializeXmlAsync<AVTransport>(new Uri(this.PresentationURL + serv.SCPDURL.Substring(1)));
                        break;
                }

            SetDefaultIconUrl();
            this.Self = this;
        }

        #endregion

        #region Private functions
        
        private void SetDefaultIconUrl()
        {
            try
            {
                Icon[] iconList = DeviceDescription.Device.IconList;
                if (iconList != null)
                {
                    foreach (Icon ic in iconList)
                    {
                        if (ic.MimeType.IndexOf("png") > -1)
                        {
                            this.DefaultIconUrl = ic.Url;
                        }
                    }
                    if (string.IsNullOrEmpty(this.DefaultIconUrl) && (iconList != null))
                        this.DefaultIconUrl = iconList[0].Url;

                    if (!string.IsNullOrEmpty(this.DefaultIconUrl))
                    {
                        if (this.DefaultIconUrl.Substring(0, 1) == "/")
                            this.DefaultIconUrl = this.DefaultIconUrl.Substring(1);
                        this.DefaultIconUrl = this.PresentationURL + this.DefaultIconUrl;
                    }
                }
            }
            catch
            {
            }
        }

        #endregion
    }
}
