﻿using Epi;
using Epi.Fields;
using ERHMS.EpiInfo.Templating.Xml;
using System.Collections.Generic;

namespace ERHMS.EpiInfo.Templating.Mapping
{
    public class DDLFieldOfCodesMapper : FieldMapper<DDLFieldOfCodes>
    {
        private const char FieldInfoSeparator = ':';

        protected override MetaFieldType? FieldType => MetaFieldType.Codes;

        protected override FieldPropertySetterCollection<DDLFieldOfCodes> PropertySetters { get; } =
            new FieldPropertySetterCollection<DDLFieldOfCodes>
            {
                { field => field.AssociatedFieldInformation, nameof(XField.RelateCondition) }
            };

        private bool MapAssociatedFieldInformation(string value, out string result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }
            bool changed = false;
            IList<string> fieldInfos = value.Split(Constants.LIST_SEPARATOR);
            for (int index = 0; index < fieldInfos.Count; index++)
            {
                string fieldInfo = fieldInfos[index];
                IList<string> chunks = fieldInfo.Split(FieldInfoSeparator);
                if (chunks.Count != 2)
                {
                    continue;
                }
                string columnName = chunks[0];
                if (!int.TryParse(chunks[1], out int fieldId))
                {
                    continue;
                }
                if (MappingContext.MapFieldId(fieldId, out fieldId))
                {
                    fieldInfos[index] = $"{columnName}{FieldInfoSeparator}{fieldId}";
                    changed = true;
                }
            }
            result = string.Join(Constants.LIST_SEPARATOR.ToString(), fieldInfos);
            return changed;
        }

        public override bool MapProperties(DDLFieldOfCodes field)
        {
            bool changed = false;
            if (MapAssociatedFieldInformation(field.AssociatedFieldInformation, out string result))
            {
                field.AssociatedFieldInformation = result;
                changed = true;
            }
            return changed;
        }

        public override bool MapAttributes(XField xField)
        {
            bool changed = false;
            if (MapAssociatedFieldInformation(xField.RelateCondition, out string result))
            {
                xField.RelateCondition = result;
                changed = true;
            }
            return changed;
        }
    }
}
