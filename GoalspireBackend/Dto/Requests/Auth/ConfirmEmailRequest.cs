﻿namespace GoalspireBackend.Dto.Requests.Auth;

public class ConfirmEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}