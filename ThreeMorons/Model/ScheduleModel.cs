﻿namespace ThreeMorons.Model
{

    /// <summary>
    /// Базовое представление пары
    /// </summary>
    public record Para()
    {
        /// <summary>
        /// Порядковый номер пары.
        /// </summary>
        public int classNumber { get; set; }
        /// <summary>
        /// Название всех пар. Пара может быть одна или две.
        /// </summary>
        public string[] NameOfClasses { get; set; }
        /// <summary>
        /// Ф.И.О. Преподавателей. Может быть одно или два
        /// </summary>
        public string[] TeacherNames { get; set; }
        /// <summary>
        /// Номера кабинетов. Может быть один или два
        /// </summary>
        public int[] ClassroomNumbers { get; set; }
    }
    /// <summary>
    /// Базовое представление дня недели
    /// </summary>
    public record DayOfWeek()
    {
        /// <summary>
        /// Пары, которые содержатся в этом дне.
        /// </summary>
        public List<Para> spisokPar { get; set; } = new List<Para>(7);
        /// <summary>
        /// Дата. (мне очень хочется написать день дня)
        /// </summary>
        public DateOnly date { get; set; }
    }
    /// <summary>
    /// Само расписание
    /// </summary>
    public record ScheduleOfWeek()
    {
        /// <summary>
        /// Дни недели с понедельника по пятницу
        /// </summary>
        public List<DayOfWeek> days { get; set; } = new List<DayOfWeek>(5);
    }
}
