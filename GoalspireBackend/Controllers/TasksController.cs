using Microsoft.AspNetCore.Mvc;

namespace GoalspireBackend.Controllers;

public class TasksController : ApiBaseController
{
    [HttpGet]
    public ActionResult GetAllTasks()
    {
        throw new NotImplementedException();
    }
    
    [HttpGet("{id}")]
    public ActionResult GetTask(Guid id)
    {
        throw new NotImplementedException();
    }
    
    [HttpPut]
    public ActionResult CreateTask()
    {
        throw new NotImplementedException();
    }
    
    [HttpPut("{id}")]
    public ActionResult UpdateTask(Guid id)
    {
        throw new NotImplementedException();
    }
    
    [HttpDelete("{id}")]
    public ActionResult DeleteTask(Guid id)
    {
        throw new NotImplementedException();
    }
}