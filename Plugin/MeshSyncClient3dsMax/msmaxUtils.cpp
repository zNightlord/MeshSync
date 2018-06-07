#include "pch.h"
#include "msmaxUtils.h"

TimeValue GetTime()
{
    return GetCOREInterface()->GetTime();
}
float ToSeconds(TimeValue tics)
{
    return TicksToSec(tics);
}
TimeValue ToTicks(float sec)
{
    return SecToTicks(sec);
}

std::wstring GetNameW(INode *n)
{
    return n->GetName();
}
std::string GetName(INode *n)
{
    return mu::ToMBS(n->GetName());
}

std::wstring GetPathW(INode *n)
{
    std::wstring ret;
    auto parent = n->GetParentNode();
    if (parent && parent->GetObjectRef()) {
        ret = GetPathW(parent);
    }
    ret += L'/';
    ret += n->GetName();
    return ret;
}
std::string GetPath(INode *n)
{
    return mu::ToMBS(GetPathW(n));
}

Object * GetTopObject(INode * n)
{
    return n->GetObjectRef();
}

Object * GetBaseObject(INode * n)
{
    return EachObject(n, [](Object*) {});
}

Modifier* FindSkin(INode *n)
{
    Modifier *ret = nullptr;
    EachModifier(n, [&ret](Object *obj, Modifier *mod) {
        if (mod->ClassID() == SKIN_CLASSID) {
            ret = mod;
        }
    });
    return ret;
}

Modifier* FindMorph(INode * n)
{
    Modifier *ret = nullptr;
    EachModifier(n, [&ret](Object *obj, Modifier *mod) {
        if (mod->ClassID() == MR3_CLASS_ID) {
            ret = mod;
        }
    });
    return ret;
}

bool IsMesh(Object *obj)
{
    return obj->SuperClassID() == GEOMOBJECT_CLASS_ID && obj->ClassID() != BONE_OBJ_CLASSID;
}
