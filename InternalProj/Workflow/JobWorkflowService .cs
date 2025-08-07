using InternalProj.Data;
using InternalProj.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace InternalProj.Workflow
{
    public class JobWorkflowService : IJobWorkflowService
    {
        private readonly ApplicationDbContext _context;

        public JobWorkflowService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Job> CreateJobWithStagesAsync(int workOrderId)
        {
            var job = new Job { WorkOrderId = workOrderId };
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            var templates = await _context.JobStageTemplates.OrderBy(t => t.Sequence).ToListAsync();

            var stages = templates.Select(t => new JobStage
            {
                JobId = job.Id,
                JobStageTemplateId = t.Id,
                Status = t.Sequence == 1 // Set first stage active if Sequence = 0
            }).ToList();

            _context.JobStages.AddRange(stages);
            await _context.SaveChangesAsync();

            return job;
        }
    }
}
