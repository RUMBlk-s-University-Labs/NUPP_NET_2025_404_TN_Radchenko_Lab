namespace Polls.Infrastructure.Models;
using System.ComponentModel.DataAnnotations;

public class IIdentifiableModel
{
    [Key]
    public Guid Id { get; set; }
}