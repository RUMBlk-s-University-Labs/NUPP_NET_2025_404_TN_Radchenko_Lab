namespace Polls.Infrastructure.Models;
using System.ComponentModel.DataAnnotations;

public interface IIdentifiableModel
{
    [Key]
    public Guid Id { get; set; }
}