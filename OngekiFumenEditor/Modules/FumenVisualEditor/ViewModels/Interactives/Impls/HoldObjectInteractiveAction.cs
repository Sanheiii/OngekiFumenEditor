﻿using OngekiFumenEditor.Base;
using OngekiFumenEditor.Base.OngekiObjects;
using OngekiFumenEditor.Base.OngekiObjects.ConnectableObject;
using OngekiFumenEditor.Base.OngekiObjects.Lane.Base;
using OngekiFumenEditor.Modules.FumenVisualEditor.Base;
using OngekiFumenEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels.Interactives.Impls
{
    public class HoldObjectInteractiveAction : DockableObjectInteractiveAction
    {
        public override IEnumerable<ConnectableObjectBase> PickDockableObjects(FumenVisualEditorViewModel editor = null)
        {
            return base.PickDockableObjects(editor);
        }

        public override void OnMoveCanvas(OngekiObjectBase obj, Point relativePoint, FumenVisualEditorViewModel editor)
        {
            base.OnMoveCanvas(obj, relativePoint, editor);

            UpdateHoldEndXGrid(obj);
        }

        public override void OnDragEnd(OngekiObjectBase obj, Point point, FumenVisualEditorViewModel editor)
        {
            base.OnDragEnd(obj, point, editor);

            editor.UndoRedoManager.ExecuteAction(LambdaUndoAction.Create("更新HoldEnd位置",
                () =>
                {
                    //UpdateHoldEndXGrid(obj);
                }, () =>
                {
                    UpdateHoldEndXGrid(obj);
                }));
        }

        private void UpdateHoldEndXGrid(OngekiObjectBase obj)
        {
            var hold = obj as Hold;
            if (hold.HoldEnd is HoldEnd end && hold.ReferenceLaneStart is ConnectableStartObject start)
            {
                if (start.CalulateXGrid(end.TGrid) is XGrid xGrid)
                    end.XGrid = xGrid;
            }
        }

    }
}
