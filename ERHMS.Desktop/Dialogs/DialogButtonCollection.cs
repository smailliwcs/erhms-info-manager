﻿using ERHMS.Desktop.Properties;
using System.Collections.Generic;

namespace ERHMS.Desktop.Dialogs
{
    public class DialogButtonCollection : List<DialogButton>
    {
        public static DialogButtonCollection Ok => new DialogButtonCollection
        {
            { ResXResources.AccessText_Ok, null, true, true }
        };

        public static DialogButtonCollection Close => new DialogButtonCollection
        {
            { ResXResources.AccessText_Close, null, true, true }
        };

        public static DialogButtonCollection YesOrNo => new DialogButtonCollection
        {
            { ResXResources.AccessText_Yes, true, true, false },
            { ResXResources.AccessText_No, false, false, true }
        };

        public static DialogButtonCollection ActionOrCancel(object content)
        {
            return new DialogButtonCollection
            {
                { content, true, true, false },
                { ResXResources.AccessText_Cancel, false, false, true }
            };
        }

        public void Add(object content, bool? result, bool isDefault, bool isCancel)
        {
            Add(new DialogButton(content, result, isDefault, isCancel));
        }
    }
}
