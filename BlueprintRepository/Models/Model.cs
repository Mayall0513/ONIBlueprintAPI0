using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlueprintRepository.Models {
    public abstract class Model {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    }
}
