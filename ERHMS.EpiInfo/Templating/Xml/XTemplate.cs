﻿using Epi;
using ERHMS.EpiInfo.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ERHMS.EpiInfo.Templating.Xml
{
    public class XTemplate : XElement
    {
        public const string DateFormat = "F";

        public static XmlWriterSettings XmlWriterSettings => new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = true
        };

        public static bool IsLevelSupported(TemplateLevel level)
        {
            switch (level)
            {
                case TemplateLevel.Project:
                case TemplateLevel.View:
                case TemplateLevel.Page:
                    return true;
                default:
                    return false;
            }
        }

        public static XTemplate Create(TemplateLevel level)
        {
            if (!IsLevelSupported(level))
            {
                throw new NotSupportedException();
            }
            return new XTemplate
            {
                Name = null,
                Description = null,
                CreateDate = DateTime.Now,
                Level = level
            };
        }

        public new string Name
        {
            get { return (string)this.GetAttributeEx(); }
            set { this.SetAttributeValueEx(value); }
        }

        public string Description
        {
            get { return (string)this.GetAttributeEx(); }
            set { this.SetAttributeValueEx(value); }
        }

        public DateTime? CreateDate
        {
            get
            {
                if (DateTime.TryParse((string)this.GetAttributeEx(), out DateTime value))
                {
                    return value;
                }
                return null;
            }
            set
            {
                this.SetAttributeValueEx(value?.ToString(DateFormat));
            }
        }

        public TemplateLevel Level
        {
            get { return TemplateLevelExtensions.Parse((string)this.GetAttributeEx()); }
            set { this.SetAttributeValueEx(value); }
        }

        public XProject XProject => Elements().OfType<XProject>().Single();
        public IEnumerable<XTable> XSourceTables => Elements(ElementNames.SourceTable).OfType<XTable>();
        public IEnumerable<XTable> XGridTables => Elements(ElementNames.GridTable).OfType<XTable>();

        public XTemplate()
            : base(ElementNames.Template) { }

        public XTemplate(XElement element)
            : this()
        {
            Add(element.Attributes());
            if (!IsLevelSupported(Level))
            {
                throw new NotSupportedException();
            }
            XElement xProject = element.Element(ElementNames.Project);
            Add(new XProject(xProject, Level));
            foreach (string elementName in ElementNames.Tables)
            {
                foreach (XElement xTable in element.Elements(elementName))
                {
                    Add(new XTable(xTable));
                }
            }
            OnCreated();
        }

        internal void OnCreated()
        {
            CreateDate = null;
            if (Level != TemplateLevel.Project)
            {
                RemoveRelateFields();
            }
        }

        private void RemoveRelateFields()
        {
            ICollection<XField> xFields = XProject.XFields
                .Where(xField => xField.FieldType == MetaFieldType.Relate)
                .ToList();
            foreach (XField xField in xFields)
            {
                xField.Remove();
            }
            foreach (XView xView in XProject.XViews)
            {
                xView.IsRelatedView = false;
            }
        }
    }
}
