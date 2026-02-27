using System.ComponentModel.DataAnnotations;

namespace WhoOwesWho.EventService.EfCore.DataModels
{
    public class EventAssignments
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public Guid UserId { get; set; }
    }
}
