using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class user
{
    public int user_id { get; set; }

    public string? username { get; set; }

    public string? account_name { get; set; }

    public string password { get; set; } = null!;

    public string? address { get; set; }

    public string? phone_number { get; set; }

    public bool? is_disabled { get; set; }

    public DateTime? create_at { get; set; }

    public string? avatar_url { get; set; }

    public DateOnly? birthday { get; set; }

    public int? role_id { get; set; }

    public int? statistic_id { get; set; }

    public string? email { get; set; }

    public int gender_id { get; set; }

    public virtual ICollection<attendance> attendances { get; set; } = new List<attendance>();

    public virtual ICollection<consultation_request> consultation_requests { get; set; } = new List<consultation_request>();

    public virtual gender gender { get; set; } = null!;

    public virtual ICollection<opening_schedule> opening_schedules { get; set; } = new List<opening_schedule>();

    public virtual role? role { get; set; }

    public virtual statistic? statistic { get; set; }

    public virtual ICollection<user_favorite_sheet> user_favorite_sheets { get; set; } = new List<user_favorite_sheet>();

    public virtual ICollection<_class> classes { get; set; } = new List<_class>();

    public virtual ICollection<document> documents { get; set; } = new List<document>();
}
