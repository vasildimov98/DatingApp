﻿namespace API.Helpers;

public class UserParams
{
    private const int MaxPageSize = 50;

    private int pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => this.pageSize;
        set => this.pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public string? Gender { get; set; }

    public string? CurrentUsername { get; set; }

    public string? OrderBy { get; set; }

    public int MinAge { get; set; } = 18;

    public int MaxAge { get; set; } = 100;

}
