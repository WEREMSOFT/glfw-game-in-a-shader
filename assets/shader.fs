#define MAX_STEPS 100
#define MAX_DIST 50.
#define MIN_DIST .01

#define TAU 6.283185
#define PI 3.141592
#define PI2 1.5707

#define MAT_DEFAULT 0
#define MAT_CHECKERS 1
#define MAT_MIRROR 2
#define MAT_GOLD 3

#define R 3. + sin(iTime)

struct Camera
{
    vec3 position;
    vec3 direction;
    vec3 lookAt;
};

float smoothMin(float a, float b, float ratio)
{
    float h = clamp(.5 + .5 * (b - a) / ratio, 0., 1.);
    return mix(b, a, h) - ratio * h * (1. - h);
}

mat2 getRotationMatrix(float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
}

float getDist2Box(vec3 p, vec3 size, float rotation)
{
    p.xz *= getRotationMatrix(rotation);

    return length(max(abs(p) - size, 0.));
}

float getDist2Sphere(vec3 p, float radius)
{
    return length(p) - radius;
}

const float sphereRadius = 1.5;

float getDistance(vec3 p)
{
    float r = R;

    float floorD = p.y + 4.;

    float angle = 0.;
    float angleIncrement = TAU / 6.;

    // distance to sphere
    vec3 sphere = vec3(sin(iTime + angle) * r, 1., cos(iTime + angle) * r);
    float ds = getDist2Sphere(p - sphere, sphereRadius);
    angle += angleIncrement;

    // distance to sphere2
    vec3 sphere2 = vec3(sin(iTime + angle) * r, 1., cos(iTime + angle) * r);
    float ds2 = getDist2Sphere(p - sphere2, sphereRadius);
    angle += angleIncrement;

    // distance to sphere3
    vec3 sphere3 = vec3(sin(iTime + angle) * r, 1., cos(iTime + angle) * r);
    float ds3 = getDist2Sphere(p - sphere3, sphereRadius);
    angle += angleIncrement;

    // distance to sphere4
    vec3 sphere4 = vec3(sin(iTime + angle) * r, 1., cos(iTime + angle) * r);
    float ds4 = getDist2Sphere(p - sphere4, sphereRadius);
    angle += angleIncrement;

    // distance to sphere5
    vec3 sphere5 = vec3(sin(iTime + angle) * r, 1., cos(iTime + angle) * r);
    float ds5 = getDist2Sphere(p - sphere5, sphereRadius);
    angle += angleIncrement;

    // distance to sphere6
    vec3 sphere6 = vec3(sin(iTime + angle) * r, 1., cos(iTime + angle) * r);
    float ds6 = getDist2Sphere(p - sphere6, sphereRadius);

    float boxDistance = getDist2Box(p - vec3(0., 1., 0.), vec3(.75), -iTime);

    float fussionRatio = .5;

    return smoothMin(smoothMin(smoothMin(smoothMin(smoothMin(smoothMin(smoothMin(ds, floorD, fussionRatio), boxDistance, fussionRatio), ds2, fussionRatio), ds3, fussionRatio), ds4, fussionRatio), ds5, fussionRatio), ds6, fussionRatio);
}

int getMaterial(vec3 p)
{
    float floorD = p.y + 4.;

    float r = R;

    float angle = 0.;
    float angleIncrement = TAU / 6.;

    // distance to sphere
    vec3 sphere = vec3(sin(iTime + angle) * r, 1., cos(iTime + angle) * r);
    float ds = getDist2Sphere(p - sphere, sphereRadius);
    angle += angleIncrement;

    // distance to sphere2
    vec3 sphere2 = vec3(sin(iTime + angle) * r, 1., cos(iTime + angle) * r);
    float ds2 = getDist2Sphere(p - sphere2, sphereRadius);
    angle += angleIncrement;

    // distance to sphere3
    vec3 sphere3 = vec3(sin(iTime + angle) * r, 1., cos(iTime + angle) * r);
    float ds3 = getDist2Sphere(p - sphere3, sphereRadius);
    angle += angleIncrement;

    // distance to sphere4
    vec3 sphere4 = vec3(sin(iTime + angle) * r, 1., cos(iTime + angle) * r);
    float ds4 = getDist2Sphere(p - sphere4, sphereRadius);
    angle += angleIncrement;

    // distance to sphere5
    vec3 sphere5 = vec3(sin(iTime + angle) * r, 1., cos(iTime + angle) * r);
    float ds5 = getDist2Sphere(p - sphere5, sphereRadius);
    angle += angleIncrement;

    // distance to sphere6
    vec3 sphere6 = vec3(sin(iTime + angle) * r, 1., cos(iTime + angle) * r);
    float ds6 = getDist2Sphere(p - sphere6, sphereRadius);

    float boxDistance = getDist2Box(p - vec3(0., 1., 0.), vec3(.75), -iTime);

    float d = min(min(min(min(min(min(min(ds, floorD), boxDistance), ds2), ds3), ds4), ds5), ds6);

    if (d == floorD)
        return MAT_CHECKERS;

    if (d == boxDistance)
        return MAT_DEFAULT;

    return MAT_MIRROR;
}

// Stable softshadow function from https://iquilezles.org/articles/rmshadows/
float softshadow(in vec3 ro, in vec3 rd, float mint, float maxt, float k)
{
    float res = 1.0;
    float ph = 1e20;
    for (float t = mint; t < maxt;)
    {
        float h = getDistance(ro + rd * t);
        if (h < 0.001)
            return 0.0;
        float y = h * h / (2.0 * ph);
        float d = sqrt(h * h - y * y);
        res = min(res, k * d / max(0.0, t - y));
        ph = h;
        t += h;
    }
    return res;
}

vec3 getNormal(vec3 p)
{
    vec2 e = vec2(0.01, 0.);

    float d = getDistance(p);

    vec3 n = d - vec3(
                     getDistance(p - e.xyy),
                     getDistance(p - e.yxy),
                     getDistance(p - e.yyx));

    return normalize(n);
}

float rayMarch(vec3 ro, vec3 rd)
{

    float dO = 0.;

    for (int i = 0; i < MAX_STEPS; i++)
    {
        float dl = getDistance(ro + rd * dO);
        dO += dl;
        if (dl < MIN_DIST || dO > MAX_DIST)
            break;
    }

    return dO;
}

float getLight(vec3 p)
{
    vec3 lightPosition = vec3(-1., 5., 6.);

    float lightMoveRadius = 7.;

    vec3 lightDirection = normalize(lightPosition - p);

    vec3 n = getNormal(p);

    float angleNormLight = dot(lightDirection, n);

    float distanceToLight = rayMarch(p + n * .1, lightDirection);

    float shadow = softshadow(p, lightDirection, 0.01, 100., 8.);

    return angleNormLight * shadow;
}

float checkers(in vec3 p)
{
    vec3 s = sign(fract(p * .5) - .5);
    return .5 - .5 * s.x * s.z;
}

vec3 GetRayDir(vec2 uv, vec3 p, vec3 l, float z)
{
    vec3 f = normalize(l - p),
         r = normalize(cross(vec3(0, 1, 0), f)),
         u = cross(f, r),
         c = f * z,
         i = c + uv.x * r + uv.y * u,
         d = normalize(i);
    return d;
}

Camera getCamera(vec2 screenPosition, float zoomFactor)
{
    vec2 mousePosition = iMouse.xy / iResolution.xy;

    Camera camera;
    camera.position = vec3(0, 1., -10.);
    camera.lookAt = vec3(0, 0, 0);

    camera.position.yz *= getRotationMatrix(-mousePosition.y * PI + 1.);
    camera.position.xz *= getRotationMatrix(-mousePosition.x * TAU);
    camera.direction = GetRayDir(screenPosition * zoomFactor, camera.position, camera.lookAt, 1.);
    return camera;
}

vec4 render(inout vec3 ro, inout vec3 rd, inout float reflectivity)
{

    float d = rayMarch(ro, rd);

    vec4 color = texture(iChannel4, rd);

    vec3 p = ro + rd * d;
    vec3 n = getNormal(p);

    vec3 texCol = vec3(checkers(p));
    vec3 col = vec3(getLight(p));

    vec3 reflection = reflect(rd, n);
    ro = p + n * MIN_DIST * 3.;
    rd = reflection;

    reflectivity = 0.;

    if (d < MAX_DIST)
    {
        int material = getMaterial(p);

        if (material == MAT_DEFAULT)
        {
            color = vec4(col, 1.0);
            reflectivity = 0.;
        }
        else if (material == MAT_CHECKERS)
        {
            color = vec4(mix(col, texCol, .5), 1.0);
            reflectivity = .3;
        }
        else if (material == MAT_MIRROR)
        {

            color = vec4(0.);
            reflectivity = .9;
        }

        return color;
    }

    return color;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord)
{
    vec2 uv = (fragCoord - .5 * iResolution.xy) / iResolution.y;

    Camera camera = getCamera(uv, 2.);

    vec3 ro = camera.position;
    vec3 rd = camera.direction;
    float reflectivity = 0.;
    vec4 difuse = render(ro, rd, reflectivity);
    vec4 filt = vec4(1.);

    const int MAX_BOUNCES = 3;

    vec4 reflection = vec4(0.);

    for (int i = 0; i < MAX_BOUNCES; i++)
    {
        filt *= reflectivity;
        vec4 bounce = filt * render(ro, rd, reflectivity);
        difuse += bounce;
    }

    fragColor = difuse;
}
