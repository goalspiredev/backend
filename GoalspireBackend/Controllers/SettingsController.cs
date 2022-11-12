﻿using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

public class SettingsController : ApiBaseController
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult Get()
    {
        throw new NotImplementedException();
    }
    
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult Modify()
    {
        throw new NotImplementedException();
    }
}