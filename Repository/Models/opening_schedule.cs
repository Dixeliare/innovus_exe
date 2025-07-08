using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class opening_schedule
{
    public int opening_schedule_id { get; set; }

    public string? subject { get; set; }

    public string? class_code { get; set; }

    public DateOnly? opening_day { get; set; }

    public DateOnly? end_date { get; set; }

    public string? schedule { get; set; }

    public int? student_quantity { get; set; }

    public bool? is_advanced_class { get; set; }

    public int? teacher_user_id { get; set; }

    public int instrument_id { get; set; }

    public virtual instrument instrument { get; set; } = null!;

    public virtual user? teacher_user { get; set; }

    public virtual ICollection<user> users { get; set; } = new List<user>();
}
