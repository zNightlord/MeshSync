#pragma once
#include "msIdentifier.h"

namespace ms {

enum class EntityType
{
    Unknown,
    Transform,
    Camera,
    Light,
    Mesh,
    Points,
};

class Entity
{
public:
    using Type = EntityType;

    int id = InvalidID;
    int host_id = InvalidID;
    std::string path;

protected:
    Entity();
    virtual ~Entity();
public:
    msDefinePool(Entity);
    static std::shared_ptr<Entity> create(std::istream& is);
    virtual Type getType() const;
    virtual bool isGeometry() const;
    virtual void serialize(std::ostream& os) const;
    virtual void deserialize(std::istream& is);
    virtual void resolve();
    virtual bool strip(const Entity& base);
    virtual bool merge(const Entity& base);
    virtual bool diff(const Entity& e1, const Entity& e2);
    virtual bool lerp(const Entity& e1, const Entity& e2, float t);
    virtual bool genVelocity(const Entity& prev); // todo
    virtual void clear();
    virtual uint64_t hash() const;
    virtual uint64_t checksumTrans() const;
    virtual uint64_t checksumGeom() const;
    virtual std::shared_ptr<Entity> clone();

    Identifier getIdentifier() const;
    bool isRoot() const;
    bool identify(const Identifier& v) const;
    void getParentPath(std::string& dst) const;
    void getName(std::string& dst) const;
};
msSerializable(Entity);
msDeclPtr(Entity);


struct TransformDataFlags
{
    uint32_t unchanged : 1;
    uint32_t has_transform : 3; // TRS
    uint32_t has_visible : 1;
    uint32_t has_visible_hierarchy : 1;
    uint32_t has_layer : 1;
    uint32_t has_reference: 1;

    TransformDataFlags()
    {
        *(uint32_t*)this = ~0x1u;
    }
};

class Transform : public Entity
{
using super = Entity;
public:
    TransformDataFlags td_flags;
    float3   position = float3::zero();
    quatf    rotation = quatf::identity();
    float3   scale = float3::one();
    int index = 0;

    bool visible = true;
    bool visible_hierarchy = true;
    int layer = 0;

    std::string reference;

    // non-serializable
    int order = 0;
    Transform *parent = nullptr;
    float4x4 local_matrix = float4x4::identity();
    float4x4 global_matrix = float4x4::identity();

protected:
    Transform();
    ~Transform() override;
public:
    msDefinePool(Transform);
    static std::shared_ptr<Transform> create(std::istream& is);
    Type getType() const override;
    void serialize(std::ostream& os) const override;
    void deserialize(std::istream& is) override;
    void clear() override;
    uint64_t checksumTrans() const override;
    bool diff(const Entity& e1, const Entity& e2) override;
    bool lerp(const Entity& src1, const Entity& src2, float t) override;
    EntityPtr clone() override;

    float4x4 toMatrix() const;
    void assignMatrix(const float4x4& v);
    void applyMatrix(const float4x4& v);
};
msSerializable(Transform);
msDeclPtr(Transform);


struct CameraDataFlags
{
    uint32_t unchanged : 1;
    uint32_t has_is_ortho : 1;
    uint32_t has_fov : 1;
    uint32_t has_near_plane : 1;
    uint32_t has_far_plane : 1;
    uint32_t has_focal_length : 1;
    uint32_t has_sensor_size : 1;
    uint32_t has_lens_shift : 1;
    uint32_t has_layer_mask : 1;

    CameraDataFlags()
    {
        *(uint32_t*)this = ~0x1u;
    }
};

class Camera : public Transform
{
using super = Transform;
public:
    CameraDataFlags cd_flags;
    bool is_ortho = false;
    float fov = 30.0f;
    float near_plane = 0.3f;
    float far_plane = 1000.0f;

    // for physical camera
    float focal_length = 0.0f;
    float2 sensor_size = float2::zero();
    float2 lens_shift = float2::zero();

    int layer_mask = ~0;

protected:
    Camera();
    ~Camera() override;
public:
    msDefinePool(Camera);
    Type getType() const override;
    void serialize(std::ostream& os) const override;
    void deserialize(std::istream& is) override;
    bool diff(const Entity& e1, const Entity& e2) override;
    bool lerp(const Entity& src1, const Entity& src2, float t) override;
    void clear() override;
    uint64_t checksumTrans() const override;
    EntityPtr clone() override;
};
msSerializable(Camera);
msDeclPtr(Camera);



struct LightDataFlags
{
    uint32_t unchanged : 1;
    uint32_t has_light_type : 1;
    uint32_t has_shadow_type : 1;
    uint32_t has_color : 1;
    uint32_t has_intensity : 1;
    uint32_t has_range : 1;
    uint32_t has_spot_angle : 1;
    uint32_t has_layer_mask : 1;

    LightDataFlags()
    {
        *(uint32_t*)this = ~0x1u;
    }
};

class Light : public Transform
{
using super = Transform;
public:
    enum class LightType
    {
        Unknown = -1,
        Spot,
        Directional,
        Point,
        Area,
    };
    enum class ShadowType
    {
        Unknown = -1,
        None,
        Hard,
        Soft,
    };

    LightDataFlags ld_flags;
    LightType light_type = LightType::Directional;
    ShadowType shadow_type = ShadowType::Unknown;
    float4 color = float4::one();
    float intensity = 1.0f;
    float range = 0.0f;
    float spot_angle = 30.0f; // for spot light

    int layer_mask = ~0;

protected:
    Light();
    ~Light() override;
public:
    msDefinePool(Light);
    Type getType() const override;
    void serialize(std::ostream& os) const override;
    void deserialize(std::istream& is) override;
    bool diff(const Entity& e1, const Entity& e2) override;
    bool lerp(const Entity& src1, const Entity& src2, float t) override;
    void clear() override;
    uint64_t checksumTrans() const override;
    EntityPtr clone() override;
};
msSerializable(Light);
msDeclPtr(Light);


class Mesh;
msDeclPtr(Mesh);


// Points
struct PointsDataFlags
{
    uint32_t unchanged : 1;
    uint32_t has_points : 1;
    uint32_t has_rotations : 1;
    uint32_t has_scales : 1;
    uint32_t has_colors : 1;
    uint32_t has_velocities : 1;
    uint32_t has_ids : 1;

    PointsDataFlags()
    {
        *(uint32_t*)this = 0;
    }
};

struct PointsData
{
    PointsDataFlags pd_flags;
    float time = -1.0f;
    RawVector<float3> points;
    RawVector<quatf>  rotations;
    RawVector<float3> scales;
    RawVector<float4> colors;
    RawVector<float3> velocities;
    RawVector<int>    ids;

protected:
    PointsData();
    ~PointsData();
public:
    msDefinePool(PointsData);
    static std::shared_ptr<PointsData> create(std::istream& is);

    void serialize(std::ostream& os) const;
    void deserialize(std::istream& is);
    void clear();
    uint64_t hash() const;
    uint64_t checksumGeom() const;
    bool lerp(const PointsData& src1, const PointsData& src2, float t);
    EntityPtr clone();

    void setupPointsDataFlags();

    void getBounds(float3& center, float3& extents);
};
msSerializable(PointsData);
msDeclPtr(PointsData);

class Points : public Transform
{
using super = Transform;
public:
    // Transform::reference is used for reference for Mesh
    std::vector<PointsDataPtr> data;

protected:
    Points();
    ~Points() override;
public:
    msDefinePool(Points);
    Type getType() const override;
    bool isGeometry() const override;
    void serialize(std::ostream& os) const override;
    void deserialize(std::istream& is) override;
    void clear() override;
    uint64_t hash() const override;
    uint64_t checksumGeom() const override;
    bool lerp(const Entity& src1, const Entity& src2, float t) override;
    EntityPtr clone() override;

    void setupPointsDataFlags();
};
msSerializable(Points);
msDeclPtr(Points);

} // namespace ms
