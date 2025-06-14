﻿using ODEliteTracker.Models.Spansh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh
{
    public sealed class ExplorationTargetViewModel
    {
        public ExplorationTargetViewModel(ExplorationTarget target)
        {
            Target = target;
            BodiesInfo = target.BodiesInfo?.Select(x => new BodiesInfoViewModel(x)).ToList() ?? [];
        }

        public ExplorationTarget Target { get; }

        public string SystemName => Target.SystemName;
        public string? Property1 => Target.Property1;
        public string? Property2 => Target.Property2;
        public string? Property3 => Target.Property3;
        public string? Property4 => Target.Property4;
        public List<BodiesInfoViewModel> BodiesInfo { get; private set; }
    }
}
