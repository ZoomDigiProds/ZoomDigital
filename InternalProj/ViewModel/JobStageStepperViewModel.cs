namespace InternalProj.ViewModel
{

        public class JobStageStepperViewModel
        {
            public int JobId { get; set; }
            public int Sequence { get; set; }
            public string StageName { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsCurrent { get; set; }
        public bool InProgress { get; internal set; }
        public int JobStageTemplateId { get; internal set; }
        public int Id { get; internal set; }
    }
    


}
