using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNote.Models
{
    public class PianoKey
    {
        public int NoteNumber { get; set; } // MIDI номер ноты
        public string NoteName { get; set; } // Название ноты (например, C4, D#5)
        public bool IsBlackKey { get; set; } // Является ли черной клавишей
    }
}
