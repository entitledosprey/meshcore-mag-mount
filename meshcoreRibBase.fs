FeatureScript 3083;
import(path : "onshape/std/common.fs", version : "3083.0");

// ===========================================================================
// MeshCore rib-clearing roof brace
//
// A SEPARATE design from meshcoreMagMount.fs, which is unchanged. This is an
// alternative base for a roof whose pan carries small raised stiffening ribs -
// the sort you cannot sit a flat plate on without it rocking.
//
// The idea is that NOTHING touches the roof except the magnet pads. Everything
// else is lifted `ribClear` (0.250 in) clear, so the ribs pass underneath and
// their PITCH never has to be known - which matters, because it cannot be read
// off a photo and guessing it would put the relief channels in the wrong place.
// The pads are deliberately narrow in X (about 0.75 in) and long in Y, so you
// orient the brace with X across the ribs and each pad drops into a flat land.
//
// It also buys a lot of wind: magnets 12.5 in apart at the corners of a square
// sit at a 224 mm radius against the cross base's 120, and tools/loads.py puts
// the tipping speed at 134 mph instead of 101.
//
// The clutch ear is identical to the one in meshcoreMagMount.fs, so the SAME
// yaw post, head and top plate fit this base unchanged.
//
// Paste the whole file into its own Feature Studio.
// ===========================================================================

const RB_RING_EXTEND = 1.0;

// The clutch ear is built along Y, exactly as in meshcoreMagMount.fs, and then
// yawed by this. It is not a preference - it is forced by the roof.
// The feet are narrow in X so they drop between the ribs, which means the ribs
// run along Y; roof panel ribs run UP-SLOPE, so Y is the fall line. Plumbing
// the mast means tilting toward +-Y, and tilting toward Y means the clutch axis
// has to lie along X. Left along Y the mount tilts ACROSS the slope, which is
// the one direction that cannot help.
// The other base does not care - it is round-ish and you just spin it on the
// roof - but this one cannot be spun without putting the feet across the ribs.
const RB_CLUTCH_YAW = 90.0;

const RB_MAGSPAN_BOUNDS = { (millimeter) : [120.0, 280.0, 500.0] } as LengthBoundSpec;
const RB_RIBCLEAR_BOUNDS = { (millimeter) : [2.0, 6.35, 25.0] } as LengthBoundSpec;
const RB_BED_BOUNDS = { (millimeter) : [150.0, 350.0, 700.0] } as LengthBoundSpec;
const RB_PADW_BOUNDS = { (millimeter) : [14.0, 24.0, 60.0] } as LengthBoundSpec;
const RB_MAGD_BOUNDS = { (millimeter) : [5.0, 12.9, 40.0] } as LengthBoundSpec;
const RB_MAGT_BOUNDS = { (millimeter) : [1.0, 3.175, 15.0] } as LengthBoundSpec;
const RB_YOKET_BOUNDS = { (millimeter) : [0.0, 3.0, 12.0] } as LengthBoundSpec;
const RB_MAGWALL_BOUNDS = { (millimeter) : [1.2, 3.0, 10.0] } as LengthBoundSpec;
const RB_MAGPITCH_BOUNDS = { (millimeter) : [14.0, 32.0, 90.0] } as LengthBoundSpec;
const RB_PERCORNER_BOUNDS = { (unitless) : [1, 2, 4] } as IntegerBoundSpec;
const RB_ARMW_BOUNDS = { (millimeter) : [10.0, 24.0, 80.0] } as LengthBoundSpec;
const RB_ARMTIP_BOUNDS = { (millimeter) : [8.0, 16.0, 60.0] } as LengthBoundSpec;
const RB_ARMTOP_BOUNDS = { (millimeter) : [10.0, 30.0, 80.0] } as LengthBoundSpec;
const RB_HUBR_BOUNDS = { (millimeter) : [20.0, 38.0, 90.0] } as LengthBoundSpec;

// --- clutch, copied dimension for dimension from meshcoreMagMount.fs so one
// --- yaw post fits either base. Change these here and you must change them there.
const RB_CLUTCHZ_BOUNDS = { (millimeter) : [20.0, 54.0, 140.0] } as LengthBoundSpec;
const RB_CLUTCHOD_BOUNDS = { (millimeter) : [20.0, 42.0, 90.0] } as LengthBoundSpec;
const RB_CLUTCHID_BOUNDS = { (millimeter) : [8.0, 28.0, 60.0] } as LengthBoundSpec;
const RB_TEETH_BOUNDS = { (unitless) : [8, 36, 120] } as IntegerBoundSpec;
const RB_TONGUET_BOUNDS = { (millimeter) : [4.0, 12.0, 30.0] } as LengthBoundSpec;
const RB_EART_BOUNDS = { (millimeter) : [4.0, 12.0, 30.0] } as LengthBoundSpec;
const RB_BOLTD_BOUNDS = { (millimeter) : [3.0, 6.2, 12.0] } as LengthBoundSpec;
const RB_SLEEVED_BOUNDS = { (millimeter) : [0.0, 0.0, 18.0] } as LengthBoundSpec;
const RB_NUTAF_BOUNDS = { (millimeter) : [5.0, 10.3, 20.0] } as LengthBoundSpec;
const RB_NUTDEP_BOUNDS = { (millimeter) : [2.0, 4.0, 14.0] } as LengthBoundSpec;
const RB_EDGE_BOUNDS = { (millimeter) : [0.0, 0.8, 4.0] } as LengthBoundSpec;

export enum RbNutStyle
{
    annotation { "Name" : "Captive nyloc in a hex pocket" }
    CAPTIVE,
    annotation { "Name" : "Plain through hole, nut outside" }
    PLAIN
}

// ------------------------------------------------------------- helpers
function mm(v)
{
    return v * millimeter;
}

function p3(x, y, z)
{
    return vector(x, y, z) * millimeter;
}

function block(context is Context, id is Id, name is string, c1 is array, c2 is array) returns Query
{
    fCuboid(context, id + name, {
                "corner1" : p3(c1[0], c1[1], c1[2]),
                "corner2" : p3(c2[0], c2[1], c2[2])
            });
    return qCreatedBy(id + name, EntityType.BODY);
}

// Cylinder along Z.
function cylZ(context is Context, id is Id, name is string, x, y, z0, z1, r) returns Query
{
    fCylinder(context, id + name, {
                "bottomCenter" : p3(x, y, z0),
                "topCenter" : p3(x, y, z1),
                "radius" : mm(r)
            });
    return qCreatedBy(id + name, EntityType.BODY);
}

// Cylinder along Y - the clutch axis.
function cylY(context is Context, id is Id, name is string, x, z, y0, y1, r) returns Query
{
    fCylinder(context, id + name, {
                "bottomCenter" : p3(x, y0, z),
                "topCenter" : p3(x, y1, z),
                "radius" : mm(r)
            });
    return qCreatedBy(id + name, EntityType.BODY);
}

// UNION and SUBTRACTION keep the target's identity; INTERSECTION does not.
// Returning qUnion of every input means the survivor is found whichever
// identity won the boolean.
function unionInto(context is Context, id is Id, name is string, target is Query, tools is array) returns Query
{
    if (size(tools) == 0)
        return target;
    const all = concatenateArrays([[target], tools]);
    opBoolean(context, id + name, {
                "tools" : qUnion(all),
                "operationType" : BooleanOperationType.UNION
            });
    return qUnion(all);
}

function cutWith(context is Context, id is Id, name is string, targets is Query, tools is array)
{
    if (size(tools) == 0)
        return;
    opBoolean(context, id + name, {
                "targets" : targets,
                "tools" : qUnion(tools),
                "operationType" : BooleanOperationType.SUBTRACTION
            });
}

// Hexagonal prism along Z, across-flats = af. A block with its corners
// trimmed off: subtraction preserves the target's identity, so the query
// returned here always finds the result. Building it as an INTERSECTION
// would hand back a body this query cannot see, and the tool would survive
// the later cut as a stray solid.
function hexPrismZ(context is Context, id is Id, name is string, cx, cy, z0, z1, af) returns Query
{
    const r = af / 2.0;
    const big = af;
    var body = block(context, id, name ~ "b",
            [cx - big, cy - big, z0], [cx + big, cy + big, z1]);
    var trims = [];
    for (var k = 0; k < 6; k += 1)
    {
        const t = block(context, id, name ~ "t" ~ toString(k),
                [cx - 2 * big, cy + r, z0 - 1], [cx + 2 * big, cy + 3 * big, z1 + 1]);
        opTransform(context, id + (name ~ "tr" ~ toString(k)), {
                    "bodies" : t,
                    "transform" : rotationAround(line(p3(cx, cy, 0), vector(0, 0, 1)),
                            (k * 60.0) * degree)
                });
        trims = append(trims, t);
    }
    cutWith(context, id, name ~ "cut", body, trims);
    return body;
}

function toothDepth(od is number, idia is number, count is number) returns number
{
    // Derived from the radius the cutter ACTUALLY reaches, not the OD. Sizing
    // it off the OD and then extending the cutter past the OD pulls the
    // width-equals-pitch radius back inside the cutter's own range, which is
    // the tangency this is meant to avoid.
    return PI * (od / 2.0 + RB_RING_EXTEND) / count * 1.03;
}

function vToothRing(context is Context, id is Id, name is string,
    count is number, od, idia, depth, spinDeg) returns Query
{
    const s = depth * sqrt(2.0);
    const r0 = max(idia / 2.0 - 1.0, 0.6);
    // Runs 1 mm PAST the OD so each prism's end face exits the body it is
    // cutting. Ending exactly at od/2 puts that face on the part's own
    // cylindrical surface - a tangent contact, and the fatter the teeth get the
    // bigger it is. The mating boss is still only od/2, so the extra groove is
    // just clearance.
    const r1 = od / 2.0 + RB_RING_EXTEND;

    // A RADIAL SEGMENT, from just inside the ID out to the OD - NOT a full
    // chord. Full chords all run through the axis, so half the ring's bodies
    // pile up inside one sub-millimetre blob at r = 0. Onshape fails that pile
    // and reports only "Boolean operation failed to return a valid part",
    // naming neither the operation nor the reason. Starting at r0 also means
    // one cutter per tooth rather than one per two, which costs nothing.
    fCuboid(context, id + (name ~ "seed"), {
                "corner1" : p3(r0, -s / 2.0, -s / 2.0),
                "corner2" : p3(r1, s / 2.0, s / 2.0)
            });
    const seed = qCreatedBy(id + (name ~ "seed"), EntityType.BODY);

    // Roll 45 degrees: the square becomes a diamond whose lower vertex sits
    // at -depth and whose waist is exactly at z = 0, the face plane.
    opTransform(context, id + (name ~ "roll"), {
                "bodies" : seed,
                "transform" : rotationAround(line(p3(0, 0, 0), vector(1, 0, 0)), 45 * degree)
            });

    if (spinDeg != 0)
    {
        opTransform(context, id + (name ~ "spin"), {
                    "bodies" : seed,
                    "transform" : rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)), spinDeg * degree)
                });
    }

    var xf = [];
    var nm = [];
    for (var k = 1; k < count; k += 1)
    {
        xf = append(xf, rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)),
                    (k * 360.0 / count) * degree));
        nm = append(nm, name ~ "p" ~ toString(k));
    }
    var all = [seed];
    if (size(xf) > 0)
    {
        opPattern(context, id + (name ~ "pat"), {
                    "entities" : seed,
                    "transforms" : xf,
                    "instanceNames" : nm
                });
        all = append(all, qCreatedBy(id + (name ~ "pat"), EntityType.BODY));
    }
    return qUnion(all);
}

// Move a ring built at the origin with its axis along +Z onto a face whose
// outward normal is one of +Z, +Y or -Y. rotX is what maps local +Z there.
function placeRing(context is Context, id is Id, name is string, ring is Query,
    rotXDeg, at is Vector)
{
    if (rotXDeg != 0)
    {
        opTransform(context, id + (name ~ "rx"), {
                    "bodies" : ring,
                    "transform" : rotationAround(line(p3(0, 0, 0), vector(1, 0, 0)), rotXDeg * degree)
                });
    }
    opTransform(context, id + (name ~ "mv"), {
                "bodies" : ring,
                "transform" : transform(at)
            });
}

// Edge picked by WHERE IT IS, not by what made it. After this many booleans a
// qCreatedBy edge query is worthless; a horizontal ring at a known height
// survives everything.
function edgeIsHorizontalAt(context is Context, e is Query, zTarget) returns boolean
{
    const l = evEdgeTangentLine(context, { "edge" : e, "parameter" : 0.5 });
    return abs(l.direction[2]) < 0.01 && abs(l.origin[2] / millimeter - zTarget) < 0.05;
}

function chamferEdgeSet(context is Context, id is Id, edges is Query, amt) returns boolean
{
    opChamfer(context, id, {
                "entities" : edges,
                "chamferType" : ChamferType.EQUAL_OFFSETS,
                "width" : mm(amt)
            });
    return true;
}

// opFillet returns nothing, so a bare `try silent` around it hides every
// failure. Wrapping it in a function that returns true gives a value to test.
function filletEdgeSet(context is Context, id is Id, edges is Query, amt) returns boolean
{
    opFillet(context, id, { "entities" : edges, "radius" : mm(amt) });
    return true;
}

// Break or round the ring of edges at one height. The WHOLE ring must go in a
// single operation: done one edge at a time, each corner arc then has to blend
// into faces that already exist at both its ends, and it fails. The per-edge
// loop is only a fallback for when the ring genuinely will not take the size.
function ringAt(context is Context, id is Id, name is string, body is Query,
    zTarget, amt, asFillet is boolean, minLen)
{
    if (amt <= 0)
        return;
    var picks = [];
    for (var e in evaluateQuery(context, body->qOwnedByBody(EntityType.EDGE)))
    {
        const ok = try silent(edgeIsHorizontalAt(context, e, zTarget));
        if (ok != true)
            continue;
        // Skip small rims. A height filter alone also catches every hole mouth
        // that opens at that height - counterbores, magnet pockets, the N hole.
        // None of those want rounding, and a 3 mm fillet on a counterbore rim
        // sitting 1.6 mm from the flange edge cannot be built at all: the ring
        // op fails and so does the per-edge fallback.
        const len = try silent(evLength(context, { "entities" : e }));
        if (len == undefined || len < mm(minLen))
            continue;
        picks = append(picks, e);
    }
    if (size(picks) == 0)
        return;

    var one = false;
    if (asFillet)
        one = try silent(filletEdgeSet(context, id + (name ~ "r"), qUnion(picks), amt)) == true;
    else
        one = try silent(chamferEdgeSet(context, id + (name ~ "r"), qUnion(picks), amt)) == true;
    if (one)
        return;

    for (var i = 0; i < size(picks); i += 1)
    {
        if (asFillet)
            try silent(filletEdgeSet(context, id + (name ~ toString(i)), picks[i], amt));
        else
            try silent(chamferEdgeSet(context, id + (name ~ toString(i)), picks[i], amt));
    }
}

annotation { "Feature Type Name" : "MeshCore rib brace" }
export const meshcoreRibBase = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Group Name" : "Footprint", "Collapsed By Default" : false }
        {
            // Magnet centre to magnet centre along one edge of the square. The
            // corners then sit at magSpan/sqrt(2) from the axis, which is what
            // the wind rating actually keys on.
            annotation { "Name" : "Magnet span (corner to corner along an edge)" }
            isLength(definition.magSpan, RB_MAGSPAN_BOUNDS);

            // THE WHOLE POINT. Everything except the magnet pads is lifted this
            // far off the roof, so the pan's stiffening ribs pass underneath and
            // their pitch never has to be known.
            annotation { "Name" : "Rib clearance under the brace" }
            isLength(definition.ribClear, RB_RIBCLEAR_BOUNDS);

            annotation { "Name" : "Printer bed, for the size warning" }
            isLength(definition.bedSize, RB_BED_BOUNDS);

            annotation { "Name" : "Arm width" }
            isLength(definition.armW, RB_ARMW_BOUNDS);

            annotation { "Name" : "Arm top height at the hub" }
            isLength(definition.armTopH, RB_ARMTOP_BOUNDS);

            // The arm tapers to this at the post, and the post is this tall.
            // The bending moment falls to zero at the magnet, so carrying full
            // depth all the way out is 125 cm3 of ASA doing nothing.
            annotation { "Name" : "Arm top height at the post" }
            isLength(definition.armTipH, RB_ARMTIP_BOUNDS);

            annotation { "Name" : "Hub radius" }
            isLength(definition.hubR, RB_HUBR_BOUNDS);

        }

        annotation { "Group Name" : "Magnets", "Collapsed By Default" : false }
        {
            // Narrow in X on purpose: this is the dimension that has to fit
            // between two ribs, and 19 mm is 0.75 in.
            annotation { "Name" : "Pad width across the ribs" }
            isLength(definition.padW, RB_PADW_BOUNDS);

            annotation { "Name" : "Magnets per corner" }
            isInteger(definition.magPerCorner, RB_PERCORNER_BOUNDS);

            annotation { "Name" : "Magnet spacing along the pad" }
            isLength(definition.magPitch, RB_MAGPITCH_BOUNDS);

            annotation { "Name" : "Magnet diameter" }
            isLength(definition.magD, RB_MAGD_BOUNDS);

            annotation { "Name" : "Magnet thickness" }
            isLength(definition.magT, RB_MAGT_BOUNDS);

            annotation { "Name" : "Steel yoke thickness (0 = none)" }
            isLength(definition.yokeT, RB_YOKET_BOUNDS);

            annotation { "Name" : "Cover over the magnet pocket" }
            isLength(definition.magWall, RB_MAGWALL_BOUNDS);
        }

        annotation { "Group Name" : "Clutch - must match the yaw post", "Collapsed By Default" : true }
        {
            annotation { "Name" : "Clutch axis height" }
            isLength(definition.clutchZ, RB_CLUTCHZ_BOUNDS);

            annotation { "Name" : "Tooth ring outside diameter" }
            isLength(definition.clutchOD, RB_CLUTCHOD_BOUNDS);

            annotation { "Name" : "Tooth ring inside diameter" }
            isLength(definition.clutchID, RB_CLUTCHID_BOUNDS);

            annotation { "Name" : "Tooth count (360/count = step)" }
            isInteger(definition.clutchTeeth, RB_TEETH_BOUNDS);

            annotation { "Name" : "Tongue thickness" }
            isLength(definition.tongueT, RB_TONGUET_BOUNDS);

            annotation { "Name" : "Ear thickness" }
            isLength(definition.earT, RB_EART_BOUNDS);

            annotation { "Name" : "Axis bolt clearance" }
            isLength(definition.boltD, RB_BOLTD_BOUNDS);

            // Defaults to 0, and must. holeR below is sleeveD/2 when this is
            // set, so leaving it on bores the axis to the SLEEVE size whether or
            // not a sleeve was ever turned - which on the other base left a
            // 10.3 A/F nut with 1.6 mm2 of flat to bear on.
            annotation { "Name" : "Turned sleeve outside diameter (0 = none)" }
            isLength(definition.sleeveD, RB_SLEEVED_BOUNDS);

            annotation { "Name" : "Axis nut" }
            definition.clutchNut is RbNutStyle;

            annotation { "Name" : "Nut across flats" }
            isLength(definition.nutAF, RB_NUTAF_BOUNDS);

            annotation { "Name" : "Nut pocket depth" }
            isLength(definition.nutDepth, RB_NUTDEP_BOUNDS);

            annotation { "Name" : "Edge break" }
            isLength(definition.edgeBreak, RB_EDGE_BOUNDS);
        }
    }
    {
        const span = definition.magSpan / millimeter;
        const ribClear = definition.ribClear / millimeter;
        const bedSize = definition.bedSize / millimeter;
        const armW = definition.armW / millimeter;
        const armTopH = definition.armTopH / millimeter;
        const armTipH = definition.armTipH / millimeter;
        const hubR = definition.hubR / millimeter;

        const padW = definition.padW / millimeter;
        const nPer = definition.magPerCorner;
        const magPitch = definition.magPitch / millimeter;
        const magD = definition.magD / millimeter;
        const magT = definition.magT / millimeter;
        const yokeT = definition.yokeT / millimeter;
        const magWall = definition.magWall / millimeter;

        const clutchZ = definition.clutchZ / millimeter;
        const clutchOD = definition.clutchOD / millimeter;
        const clutchID = definition.clutchID / millimeter;
        const nTeeth = definition.clutchTeeth;
        const tongueT = definition.tongueT / millimeter;
        const earT = definition.earT / millimeter;
        const boltD = definition.boltD / millimeter;
        const sleeveD = definition.sleeveD / millimeter;
        const nutAF = definition.nutAF / millimeter;
        const nutDepth = definition.nutDepth / millimeter;
        const edgeBreak = definition.edgeBreak / millimeter;

        // ---------------- derived ----------------
        const h = span / 2.0;                       // magnet centre, per axis
        const cornerR = h * sqrt(2.0);              // magnet radius from the axis
        const pocketTop = magT + yokeT;             // pocket ceiling
        const padTop = pocketTop + magWall;         // pad is this tall
        const cDepth = toothDepth(clutchOD, clutchID, nTeeth);
        // The foot is CENTRED on the corner, so the magnets straddle it. It used
        // to reach inward only, which kept the bounding box down but left the
        // foot visibly slid off the end of the arm. Symmetry costs 1.5 in of
        // span - 143 mph down to 134 - and is worth it.
        const padSpanY = (nPer - 1) * magPitch;
        const bbX = span + padW;
        const bbY = span + padSpanY + padW;

        reportFeatureInfo(context, id,
            "Brace " ~ toString(bbX) ~ " x " ~ toString(bbY) ~ " mm, magnets at r = " ~ toString(cornerR)
            ~ " mm.  Pads " ~ toString(padW) ~ " mm across the ribs, lifted "
            ~ toString(ribClear) ~ " mm.  Clutch " ~ toString(nTeeth) ~ " teeth, "
            ~ toString(360.0 / nTeeth) ~ " deg step, " ~ toString(cDepth) ~ " mm deep.");

        // ---------------- guards ----------------
        if (max(bbX, bbY) > bedSize)
            reportFeatureWarning(context, id,
                "Brace is " ~ toString(bbX) ~ " x " ~ toString(bbY) ~ " mm and the bed is "
                ~ toString(bedSize) ~ ". Reduce the magnet span.");
        // The pad has to stand proud of the raised structure or nothing touches
        // the roof but the structure itself, and the whole design is pointless.
        if (padTop <= ribClear + 1.5)
            reportFeatureWarning(context, id,
                "Magnet pads are not taller than the rib clearance - the brace "
                ~ "would sit on its arms, not its magnets. Thicken the pocket "
                ~ "cover or reduce the clearance.");
        if (padW < magD + 2.4)
            reportFeatureWarning(context, id,
                "Pad is narrower than the magnet plus a 1.2 mm wall.");
        // THE ARM ARRIVES AT 45 DEG. Its end face is armW wide, so its two
        // corners land armW/2 * cos(45) either side of the diagonal - in BOTH
        // X and Y. If the post is narrower than that the corners hang out past
        // it in mid-air, which is what the first version did: a 30 mm arm on a
        // 19 mm pad left a 1.1 mm flag on each side.
        if (padW < armW * cos(45 * degree) + 3.0)
            reportFeatureWarning(context, id,
                "Post is too narrow for the arm to die into it. At " ~ toString(armW)
                ~ " mm the arm's end corners need a post at least "
                ~ toString(armW * cos(45 * degree) + 3.0) ~ " mm across.");
        if (armTipH < padTop + 1.0)
            reportFeatureWarning(context, id,
                "Post is shorter than the magnet pocket plus its cover.");
        if (armTipH > armTopH)
            reportFeatureWarning(context, id,
                "Arm is deeper at the post than at the hub - the taper runs the "
                ~ "wrong way and the moment is highest at the hub.");
        // Wider than about an inch and it stops reliably finding a flat land.
        if (padW > 32.0)
            reportFeatureWarning(context, id,
                "Pad is over 32 mm across the ribs. It has to drop into a flat "
                ~ "land between two ribs, so keep it near 19 mm.");
        if (padSpanY / 2.0 + padW > cornerR - hubR)
            reportFeatureWarning(context, id,
                "Magnet feet reach further inboard than the arms are long.");
        if (armTopH <= ribClear + 6.0)
            reportFeatureWarning(context, id,
                "Arms are under 6 mm deep above the rib clearance - they carry "
                ~ "the whole magnet pull back to the hub.");
        if (hubR + 4.0 > cornerR - padSpanY / 2.0)
            reportFeatureWarning(context, id, "Hub runs into the magnet pads.");
        if (clutchZ - clutchOD / 2.0 < armTopH)
            reportFeatureWarning(context, id,
                "Clutch ear dips below the hub top - the yaw post cannot be slid on.");

        // ================================================== BRACE
        const bid = id + "brace";
        var solids = [];

        // Hub, LIFTED like everything else. Its underside is the one wide
        // ceiling on the part; see the DFM note about support.
        solids = append(solids, cylZ(context, bid, "hub", 0, 0, ribClear, armTopH, hubR));

        // Buttress under the ear, same as the other base: material where the
        // ear needs it, confined to the ear's own Y band so the tongue never
        // meets it.
        const butt = block(context, bid, "butt",
                [-clutchOD / 2.0 * 0.8, -(tongueT / 2.0 + earT), ribClear],
                [clutchOD / 2.0 * 0.8, -tongueT / 2.0, clutchZ]);
        opTransform(context, bid + "buttyaw", { "bodies" : butt,
                    "transform" : rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)),
                        RB_CLUTCH_YAW * degree) });
        solids = append(solids, butt);

        // Four diagonal arms. Written out rather than patterned so every block
        // keeps a static name and stays visible to the dimension checker.
        // TAPERED, deep at the hub and shallow at the post. The bending
        // moment from the magnet pull falls to zero at the magnet, so a
        // constant-depth arm is 125 cm3 of ASA doing nothing. Cut in the arm's
        // own frame BEFORE the Z rotation, which keeps the trigonometry to one
        // line; the wedge is rotated about +Y so its underside falls as x grows.
        const tAng = atan2(armTopH - armTipH, cornerR - hubR) / degree;
        const tBig = 4.0 * cornerR;
        const a0 = block(context, bid, "arm0", [0, -armW / 2.0, ribClear],
                [cornerR, armW / 2.0, armTopH]);
        const w0 = block(context, bid, "taper0", [hubR, -armW, armTopH],
                [hubR + tBig, armW, armTopH + tBig]);
        opTransform(context, bid + "taper0r", { "bodies" : w0,
                    "transform" : rotationAround(line(p3(hubR, 0, armTopH), vector(0, 1, 0)),
                        tAng * degree) });
        cutWith(context, bid, "tapercut0", a0, [w0]);
        opTransform(context, bid + "arm0r", { "bodies" : a0,
                    "transform" : rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)), 45 * degree) });
        solids = append(solids, a0);
        const a1 = block(context, bid, "arm1", [0, -armW / 2.0, ribClear],
                [cornerR, armW / 2.0, armTopH]);
        const w1 = block(context, bid, "taper1", [hubR, -armW, armTopH],
                [hubR + tBig, armW, armTopH + tBig]);
        opTransform(context, bid + "taper1r", { "bodies" : w1,
                    "transform" : rotationAround(line(p3(hubR, 0, armTopH), vector(0, 1, 0)),
                        tAng * degree) });
        cutWith(context, bid, "tapercut1", a1, [w1]);
        opTransform(context, bid + "arm1r", { "bodies" : a1,
                    "transform" : rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)), 135 * degree) });
        solids = append(solids, a1);
        const a2 = block(context, bid, "arm2", [0, -armW / 2.0, ribClear],
                [cornerR, armW / 2.0, armTopH]);
        const w2 = block(context, bid, "taper2", [hubR, -armW, armTopH],
                [hubR + tBig, armW, armTopH + tBig]);
        opTransform(context, bid + "taper2r", { "bodies" : w2,
                    "transform" : rotationAround(line(p3(hubR, 0, armTopH), vector(0, 1, 0)),
                        tAng * degree) });
        cutWith(context, bid, "tapercut2", a2, [w2]);
        opTransform(context, bid + "arm2r", { "bodies" : a2,
                    "transform" : rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)), 225 * degree) });
        solids = append(solids, a2);
        const a3 = block(context, bid, "arm3", [0, -armW / 2.0, ribClear],
                [cornerR, armW / 2.0, armTopH]);
        const w3 = block(context, bid, "taper3", [hubR, -armW, armTopH],
                [hubR + tBig, armW, armTopH + tBig]);
        opTransform(context, bid + "taper3r", { "bodies" : w3,
                    "transform" : rotationAround(line(p3(hubR, 0, armTopH), vector(0, 1, 0)),
                        tAng * degree) });
        cutWith(context, bid, "tapercut3", a3, [w3]);
        opTransform(context, bid + "arm3r", { "bodies" : a3,
                    "transform" : rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)), 315 * degree) });
        solids = append(solids, a3);

        // Magnet pads, AXIS ALIGNED even though the arms are diagonal. The pad's
        // X extent is what has to fit between two ribs, so it must not follow
        // the arm round to 45 deg - at 45 deg a 19 x 51 pad would present 49 mm
        // across the ribs instead of 19.
        const sxs = [-1.0, 1.0];
        const sys = [-1.0, 1.0];
        // SYMMETRIC about the corner, and shaped as a stadium: a bar with a
        // round boss at each end, and each boss IS a magnet. The arm ends in the
        // middle of it, so the foot reads as a flange on the arm rather than a
        // rectangle the arm happens to cross.
        // The first version reached inward only, to keep the bounding box down.
        // That is what made it look slid off the end. Centring it costs 1.5 in
        // of span - 143 mph down to 134, against 101 for the cross base - and
        // the envelope still fits a 350 bed.
        const yHalf = padSpanY / 2.0;
        const xIn = h - padW / 2.0;
        const xOut = h + padW / 2.0;
        solids = append(solids, block(context, bid, "pad00",
                    [-xOut, -h - yHalf, 0], [-xIn, -h + yHalf, armTipH]));
        solids = append(solids, cylZ(context, bid, "padA00",
                    -h, -h - yHalf, 0, armTipH, padW / 2.0));
        solids = append(solids, cylZ(context, bid, "padB00",
                    -h, -h + yHalf, 0, armTipH, padW / 2.0));
        solids = append(solids, block(context, bid, "pad01",
                    [-xOut, h - yHalf, 0], [-xIn, h + yHalf, armTipH]));
        solids = append(solids, cylZ(context, bid, "padA01",
                    -h, h - yHalf, 0, armTipH, padW / 2.0));
        solids = append(solids, cylZ(context, bid, "padB01",
                    -h, h + yHalf, 0, armTipH, padW / 2.0));
        solids = append(solids, block(context, bid, "pad10",
                    [xIn, -h - yHalf, 0], [xOut, -h + yHalf, armTipH]));
        solids = append(solids, cylZ(context, bid, "padA10",
                    h, -h - yHalf, 0, armTipH, padW / 2.0));
        solids = append(solids, cylZ(context, bid, "padB10",
                    h, -h + yHalf, 0, armTipH, padW / 2.0));
        solids = append(solids, block(context, bid, "pad11",
                    [xIn, h - yHalf, 0], [xOut, h + yHalf, armTipH]));
        solids = append(solids, cylZ(context, bid, "padA11",
                    h, h - yHalf, 0, armTipH, padW / 2.0));
        solids = append(solids, cylZ(context, bid, "padB11",
                    h, h + yHalf, 0, armTipH, padW / 2.0));

        var brace = unionInto(context, bid, "u", solids[0], subArray(solids, 1, size(solids)));

        // ---- clutch ear, identical to meshcoreMagMount.fs ----
        // ONE ear, not two. A face spline only engages along its axis and the
        // tongue's ridges are wider than the gap between two rigid ears, so a
        // double-shear clutch has no assembly sequence at all.
        const g = tongueT / 2.0;
        const ear = cylY(context, bid, "ear", 0, clutchZ, -g - earT, -g, clutchOD / 2.0);
        opTransform(context, bid + "earyaw", { "bodies" : ear,
                    "transform" : rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)),
                        RB_CLUTCH_YAW * degree) });
        brace = unionInto(context, bid, "u2", brace, [ear]);

        var cuts = [];
        // Built along Y like the other base, then yawed as one group at the end
        // of this block. Rotating the finished bodies rather than rewriting them
        // along X keeps this code identical to the proven version - and because
        // the yaw is about Z, it leaves every tooth's phase relative to vertical
        // untouched, so the post still meshes exactly as it did.
        var clutchCuts = [];

        // Teeth into the ear's inner face: spin 0, nominal depth, no clearance.
        // The post's tongue carries half a pitch and the clearance.
        const ringEar = vToothRing(context, bid, "tA", nTeeth, clutchOD, clutchID, cDepth, 0);
        placeRing(context, bid, "tA", ringEar, -90, p3(0, -g, clutchZ));
        clutchCuts = append(clutchCuts, ringEar);
        // Inside the ID the grooves overlap and chew the face; take it away.
        clutchCuts = append(clutchCuts, cylY(context, bid, "rel", 0, clutchZ,
                    -g - cDepth - 0.3, -g + 0.1, clutchID / 2.0));

        const holeR = (sleeveD > 0.01 ? sleeveD : boltD) / 2.0;
        clutchCuts = append(clutchCuts, cylY(context, bid, "axis", 0, clutchZ,
                    -g - earT - 2.0, g + 2.0, holeR));

        if (definition.clutchNut == RbNutStyle.CAPTIVE)
        {
            const hx = hexPrismZ(context, bid, "nut", 0, 0,
                    -nutDepth / 2.0, nutDepth / 2.0, nutAF);
            opTransform(context, bid + "nutrot", { "bodies" : hx,
                        "transform" : rotationAround(line(p3(0, 0, 0), vector(1, 0, 0)), 90 * degree) });
            opTransform(context, bid + "nutmv", { "bodies" : hx,
                        "transform" : transform(p3(0, -g - earT + nutDepth / 2.0 - 0.01, clutchZ)) });
            clutchCuts = append(clutchCuts, hx);
            // The pocket opens on the ear's outboard face; open a channel
            // straight out so the nut can actually be dropped in.
            clutchCuts = append(clutchCuts, block(context, bid, "nutaccess",
                        [-nutAF / 2.0 - 0.3, -(tongueT / 2.0 + earT) - clutchOD,
                            clutchZ - nutAF / 2.0 - 0.3],
                        [nutAF / 2.0 + 0.3, -(tongueT / 2.0 + earT) + 0.1,
                            clutchZ + nutAF / 2.0 + 0.3]));
        }

        // Yaw the whole clutch group as one. Every body above was built along Y;
        // one rotation puts the axis along X without touching any of the
        // geometry that took several attempts to get right.
        opTransform(context, bid + "clutchyaw", { "bodies" : qUnion(clutchCuts),
                    "transform" : rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)),
                        RB_CLUTCH_YAW * degree) });
        cuts = concatenateArrays([cuts, clutchCuts]);

        // Magnet pockets, OPEN AT THE BOTTOM. A 0.4 mm print membrane under a
        // magnet costs about a tenth of the pull and there is none to spare on
        // a saturating roof sheet, so the magnet sits flush with the pad face
        // and is glued. Yoke goes in first, from underneath, then the magnet.
        for (var i = 0; i < 2; i += 1)
        {
            for (var j = 0; j < 2; j += 1)
            {
                for (var k = 0; k < nPer; k += 1)
                {
                    const nm = toString(i) ~ toString(j) ~ toString(k);
                    // Straddling the corner, so they land dead centre in the
                    // two round bosses of the stadium foot.
                    cuts = append(cuts, cylZ(context, bid, "mag" ~ nm,
                                sxs[i] * h,
                                sys[j] * h - padSpanY / 2.0 + k * magPitch,
                                -1.0, pocketTop, magD / 2.0));
                }
            }
        }

        cutWith(context, bid, "cuts", brace, cuts);

        // Break the edges that matter. Both are wrapped, so a fillet that will
        // not take degrades to nothing rather than failing the feature.
        ringAt(context, bid, "ground", brace, 0, edgeBreak, false, 30.0);
        ringAt(context, bid, "lift", brace, ribClear, edgeBreak, false, 30.0);
        ringAt(context, bid, "deck", brace, armTopH, edgeBreak, false, 60.0);

        setProperty(context, { "entities" : brace, "propertyType" : PropertyType.NAME,
                    "value" : "Rib brace" });
    }, {
        "magSpan" : 280.0 * millimeter,
        "ribClear" : 6.35 * millimeter,
        "bedSize" : 350.0 * millimeter,
        "armW" : 24.0 * millimeter,
        "armTopH" : 30.0 * millimeter,
        "armTipH" : 16.0 * millimeter,
        "hubR" : 38.0 * millimeter,
        "padW" : 24.0 * millimeter,
        "magPerCorner" : 2,
        "magPitch" : 32.0 * millimeter,
        "magD" : 12.9 * millimeter,
        "magT" : 3.175 * millimeter,
        "yokeT" : 3.0 * millimeter,
        "magWall" : 3.0 * millimeter,
        "clutchZ" : 54.0 * millimeter,
        "clutchOD" : 42.0 * millimeter,
        "clutchID" : 28.0 * millimeter,
        "clutchTeeth" : 36,
        "tongueT" : 12.0 * millimeter,
        "earT" : 12.0 * millimeter,
        "boltD" : 6.2 * millimeter,
        "sleeveD" : 0.0 * millimeter,
        "clutchNut" : RbNutStyle.CAPTIVE,
        "nutAF" : 10.3 * millimeter,
        "nutDepth" : 4.0 * millimeter,
        "edgeBreak" : 0.8 * millimeter
    });
