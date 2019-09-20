﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Model {
    public class TalkModel {
        public int TalkId { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 10)]
        public string Title { get; set; }
        [Required]
        [StringLength(4000, MinimumLength = 20)]
        public string Abstract { get; set; }
        [Range(100, 300)]
        public int Level { get; set; }

        public SpeakerModel Speaker { get; set; }
    }
}
