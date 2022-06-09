
#define MAX_STEPS 100
#define MAX_DIST 50.
#define SURF_DIST .01
#define TAU 6.283185
#define PI 3.141592
struct Camera
{
    vec3 position;
    vec3 direction;
    vec3 lookAt;
};

struct Torus
{
    vec2 radiouses;
    vec3 position;
};

struct Box
{
    vec3 size;
    vec3 position;
    vec3 rotation;
};

struct Sphere
{
    float radius;
    vec3 position;
};

mat2 getRotationMatrix(float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
}

float smoothMin(float a, float b, float ammount)
{
    float h = clamp(.5 + .5 * (b - a) / ammount, 0., 1.);
    return mix(b, a, h) - ammount * h * (1. - h);
}

float getSphereDistance(Sphere sphere, vec3 point)
{
    return length(point - sphere.position) - sphere.radius;
}

float getTorusDistance(Torus torus, vec3 point)
{
    vec3 position = point - torus.position;
    float x = length(position.xz) - torus.radiouses.x;
    return length(vec2(x, position.y)) - torus.radiouses.y;
}

float getBoxDistance(Box box, vec3 point)
{
    mat2 rotationMatrix = getRotationMatrix(iTime);
    vec3 position = point - box.position;
    position.xz *= rotationMatrix;
    return length(max(abs(position) - box.size, 0.));
}

float getDistance(vec3 point)
{
    Sphere sphere;
    sphere.position = vec3(0, 1., 6.);
    sphere.radius = 1.;

    Torus torus;
    torus.radiouses = vec2(2., .3);
    torus.position = vec3(-1., .5, 6.);

    Box box;
    box.size = vec3(.5);
    box.position = vec3(-5., .5, 6.);

    sphere.position.x = sin(iTime + 3.1416) * 3.;
    sphere.position.z = cos(iTime + 3.1416) * 3. + 6.;
    float distanceToSphere = getSphereDistance(sphere, point);

    sphere.position.x = sin(iTime) * 3.;
    sphere.position.z = cos(iTime) * 3. + 6.;
    float distanceToSphere2 = getSphereDistance(sphere, point);

    torus.radiouses.x = abs(sin(iTime * .2)) + 0.5;
    torus.radiouses.y = abs(sin(iTime * 0.5)) + 0.1;
    torus.position.y = abs(sin(iTime * 0.7)) + 0.5;
    float distanceToTorus = getTorusDistance(torus, point);
    float distanceToPlane = point.y + 5. + sin(point.x * .5 + iTime) + cos(point.z * .5 + iTime);

    float distanceToBox = getBoxDistance(box, point);

    return min(smoothMin(smoothMin(smoothMin(distanceToSphere, distanceToSphere2, .5), distanceToTorus, .5), distanceToBox, .5), distanceToPlane);
}

vec3 getNormal(vec3 point)
{
    vec2 e = vec2(.01, 0);
    float distanceToPoint = getDistance(point);
    vec3 n = vec3(
        distanceToPoint - getDistance(point - e.xyy),
        distanceToPoint - getDistance(point - e.yxy),
        distanceToPoint - getDistance(point - e.yyx));

    return normalize(n);
}

float rayMarch(vec3 rayOrigin, vec3 rayDirection)
{
    float distanceToOrigin = 0;

    for (int i = 0; i < MAX_STEPS; i++)
    {
        vec3 point = rayOrigin + rayDirection * distanceToOrigin;
        float distanceToScene = getDistance(point);
        distanceToOrigin += distanceToScene;
        if (distanceToOrigin > MAX_DIST || distanceToScene < SURF_DIST)
            break;
    }

    return distanceToOrigin;
}

float getLight(vec3 point)
{
#define LIGHT_PATH_RADIOUS 2.
    vec3 lightPosition = vec3(3, 5, 6);
    vec3 lightPosNormalized = normalize(lightPosition - point);
    vec3 n = getNormal(point);
    float diffuse = clamp(dot(n, lightPosNormalized), 0., 1.);

    float d = rayMarch(point + n * SURF_DIST * 5., lightPosNormalized);

    if (d < length(lightPosition - point))
        diffuse *= .1;

    return diffuse;
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

Camera getCamera(vec2 mousePosition, vec2 screenPosition)
{
    Camera camera;
    camera.position = vec3(0, 20., -20);
    camera.lookAt = vec3(0, 0, 6);

    camera.position.yz *= getRotationMatrix(-mousePosition.y * PI + 1.);
    camera.position.xz *= getRotationMatrix(-mousePosition.x * TAU);
    camera.direction = GetRayDir(screenPosition, camera.position, camera.lookAt, 1.);
    return camera;
}

void mainImage(out vec4 fragColor, in vec2 fragCoord)
{
    vec2 uv = (fragCoord - .5 * iResolution.xy) / iResolution.y;
    vec2 m = iMouse.xy / iResolution.xy;

    vec3 light = vec3(0, 0, 5);

    Camera camera = getCamera(m, uv);

    float distance = rayMarch(camera.position, camera.direction);

    vec4 col = vec4(0);

    if (distance < MAX_DIST)
    {
        vec3 collisionPoint = camera.position + camera.direction * distance;

        col = vec4(vec3(getLight(collisionPoint)), 1.);
    }

    fragColor = col;
}