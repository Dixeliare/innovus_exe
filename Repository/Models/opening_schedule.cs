using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class opening_schedule
{
    public int opening_schedule_id { get; set; }

    public string class_code { get; set; } = null!;

    public DateOnly? opening_day { get; set; }

    public DateOnly? end_date { get; set; }

    public int? student_quantity { get; set; }

    public bool? is_advanced_class { get; set; }

    public int? teacher_user_id { get; set; }

    public int instrument_id { get; set; }

    public int total_sessions { get; set; }

    public virtual _class class_codeNavigation { get; set; } = null!;

    public virtual instrument instrument { get; set; } = null!;

    public virtual user? teacher_user { get; set; }

    public virtual ICollection<day_of_week_lookup> day_of_weeks { get; set; } = new List<day_of_week_lookup>();
}
