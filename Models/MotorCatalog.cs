using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class MotorCatalog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
    public required string motor_id { get; set; }
    public required string brand { get; set; }
    public required string category { get; set; }
    public required string current_380v { get; set; }
    public required string current_400v { get; set; }
    public required string current_415v { get; set; }
    public required string current_lrc { get; set; }
    public required string efficiency_1_2 { get; set; }
    public required string efficiency_3_4 { get; set; }
    public required string efficiency_full { get; set; }
    public required string frame_size { get; set; }
    public required string full_load_rpm { get; set; }
    public required string image_url { get; set; }
    public required string motor_type { get; set; }
    public required string output_hp { get; set; }
    public required string output_kw { get; set; }
    public required string power_factor_1_2 { get; set; }
    public required string power_factor_3_4 { get; set; }
    public required string power_factor_full { get; set; }
    public required string product_name { get; set; }
    public required string source_page { get; set; }
    public required string torque_break_down { get; set; }
    public required string torque_full { get; set; }
    public required string torque_locked_rotor { get; set; }
    public required string torque_pull_up { get; set; }
    public required string torque_rotor_gd2 { get; set; }
    public required string url { get; set; }
    public required string weight_kg { get; set; }
}
