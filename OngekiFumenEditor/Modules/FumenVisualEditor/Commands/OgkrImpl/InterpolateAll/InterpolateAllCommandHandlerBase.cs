﻿using Gemini.Framework.Commands;
using OngekiFumenEditor.Base.OngekiObjects.ConnectableObject;
using OngekiFumenEditor.Base.OngekiObjects.Lane.Base;
using OngekiFumenEditor.Kernel.CurveInterpolater.DefaultImpl.Factory;
using OngekiFumenEditor.Modules.FumenVisualEditor.Base;
using OngekiFumenEditor.Modules.FumenVisualEditor.ViewModels;
using OngekiFumenEditor.Utils;
using System.Collections.Generic;
using System.Linq;

namespace OngekiFumenEditor.Modules.FumenVisualEditor.Commands.OgkrImpl.InterpolateAll
{
	public abstract class InterpolateAllCommandHandlerBase<T> : CommandHandlerBase<T> where T : CommandDefinition
	{
		protected void Process(FumenVisualEditorViewModel editor, bool xGridLimit)
		{
			var fumen = editor.Fumen;

			var laneMap = new Dictionary<ConnectableStartObject, List<ConnectableStartObject>>();

			var curveFactory = xGridLimit ? XGridLimitedCurveInterpolaterFactory.Default : default;

			foreach ((var beforeLane, var genLanes) in Utils.Ogkr.InterpolateAll.Calculate(fumen, curveFactory))
				laneMap[beforeLane] = genLanes.ToList();

			var curveStarts = laneMap.Keys.ToList();

			var affactObjects = Utils.Ogkr.InterpolateAll.CalculateAffectedDockableObjects(fumen, curveStarts).ToArray();

			var redoAction = new System.Action(() => { });

			var undoAction = new System.Action(() => { });

			foreach (var item in laneMap)
			{
				var beforeLane = item.Key;
				var afterLanes = item.Value;

				redoAction += () =>
				{
					fumen.RemoveObject(beforeLane);
					fumen.AddObjects(afterLanes);
				};

				undoAction += () =>
				{
					fumen.AddObject(beforeLane);
					fumen.RemoveObjects(afterLanes);
				};
			}

			foreach (var obj in affactObjects)
			{
				var tGrid = obj.TGrid;
				var beforeXGrid = obj.XGrid;
				var beforeLane = obj.ReferenceLaneStart;

				(var afterLane, var afterXGrid) = laneMap[obj.ReferenceLaneStart]
					.Where(x => tGrid >= x.MinTGrid && tGrid <= x.MaxTGrid)
					.Select(x => (x, x.CalulateXGrid(tGrid)))
					.Where(x => x.Item2 is not null)
					.OrderBy(x => x.Item2)
					.FirstOrDefault();

				redoAction += () =>
				{
					obj.ReferenceLaneStart = afterLane as LaneStartBase;
					//obj.XGrid = afterXGrid;
				};

				undoAction += () =>
				{
					obj.ReferenceLaneStart = beforeLane;
					//obj.XGrid = beforeXGrid;
				};
			}

			editor.UndoRedoManager.ExecuteAction(LambdaUndoAction.Create("插值所有曲线轨道", redoAction, undoAction));
			Log.LogInfo($"插值计算完成,一共对 {curveStarts.Count} 条符合条件的轨道进行插值,生成了 {laneMap.Values.Select(x => x.Count).Sum()} 条新的轨道,对应 {affactObjects.Count()} 个受到影响的Tap/Hold等物件进行重新计算。");
		}
	}
}
