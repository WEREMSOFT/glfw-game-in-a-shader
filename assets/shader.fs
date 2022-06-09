
#define MAX_STEPS 100
#define MAX_DIST 100.
#define SURF_DIST .01
#define TAU 6.283185
#define PI 3.141592

struct Camera
{
    vec3 position;
    vec3 direction;
    vec3 lookAt;
};

struct CollisionInfo
{
    float distance;
    vec4 color;
};

struct Shape
{
    vec3 position;
    vec4 color;
    vec3 rotation;
};

struct Torus
{
    Shape parent;
    vec2 radiouses;
};

struct Box
{
    Shape parent;
    vec3 size;
};

struct Sphere
{
    Shape parent;
    float radius;
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

CollisionInfo getSphereDistance(Sphere sphere, vec3 point)
{
    CollisionInfo returnValue;
    returnValue.color = sphere.parent.color;
    returnValue.distance = length(point - sphere.parent.position) - sphere.radius;
    return returnValue;
}

CollisionInfo getTorusDistance(Torus torus, vec3 point)
{
    CollisionInfo returnValue;
    returnValue.color = torus.parent.color;
    vec3 position = point - torus.parent.position;
    float x = length(position.xz) - torus.radiouses.x;
    returnValue.distance = length(vec2(x, position.y)) - torus.radiouses.y;
    return returnValue;
}

CollisionInfo getBoxDistance(Box box, vec3 point)
{
    CollisionInfo returnValue;
    returnValue.color = box.parent.color;
    mat2 rotationMatrix = getRotationMatrix(iTime);
    vec3 position = point - box.parent.position;
    position.xz *= rotationMatrix;

    returnValue.distance = length(max(abs(position) - box.size, 0.));
    return returnValue;
}

CollisionInfo getDistance(vec3 point)
{
    CollisionInfo returnValue;
    returnValue.color = vec4(1.);

    Sphere sphere;
    sphere.parent.position = vec3(0, 1., 6.);
    sphere.radius = 1.;
    sphere.parent.color = vec4(1., 1., 0, 1.);

    Torus torus;
    torus.radiouses = vec2(2., .3);
    torus.parent.position = vec3(-1., .5, 6.);
    torus.parent.color = vec4(1., 0, 0, 1.);

    Box box;
    box.size = vec3(.5);
    box.parent.position = vec3(-5., .5, 6.);
    box.parent.color = vec4(1., 0, 1., 1.);

    sphere.parent.position.x = sin(iTime + PI) * 3.;
    sphere.parent.position.z = cos(iTime + PI) * 3. + 6.;
    sphere.parent.color = vec4(1., 1., 0, 1.);
    CollisionInfo distanceToSphere = getSphereDistance(sphere, point);

    sphere.parent.position.x = sin(iTime) * 3.;
    sphere.parent.position.z = cos(iTime) * 3. + 6.;
    CollisionInfo distanceToSphere2 = getSphereDistance(sphere, point);

    torus.radiouses.x = abs(sin(iTime * .2)) + 0.5;
    torus.radiouses.y = abs(sin(iTime * 0.5)) + 0.1;
    torus.parent.position.y = abs(sin(iTime * 0.7)) + 0.5;
    CollisionInfo distanceToTorus = getTorusDistance(torus, point);
    float distanceToPlane = point.y + 5. + sin(point.x * .5 + iTime) + cos(point.z * .5 + iTime);

    CollisionInfo distanceToBox = getBoxDistance(box, point);

    returnValue = distanceToSphere;

    if (distanceToSphere.distance < distanceToSphere2.distance)
    {
        returnValue = distanceToSphere;
    }

    if (distanceToTorus.distance < returnValue.distance)
    {
        returnValue = distanceToTorus;
    }

    if (distanceToBox.distance < returnValue.distance)
    {
        returnValue = distanceToBox;
    }

    if (distanceToPlane < returnValue.distance)
    {
        returnValue.distance = distanceToPlane;
        returnValue.color = vec4(1.);
    }

    // returnValue.distance = min(smoothMin(smoothMin(smoothMin(distanceToSphere.distance, distanceToSphere2.distance, .5), distanceToTorus.distance, .5), distanceToBox.distance, .5), distanceToPlane);
    return returnValue;
}

vec3 getNormal(vec3 point)
{
    vec2 e = vec2(.01, 0);
    CollisionInfo distanceToPoint = getDistance(point);
    vec3 n = vec3(
        distanceToPoint.distance - getDistance(point - e.xyy).distance,
        distanceToPoint.distance - getDistance(point - e.yxy).distance,
        distanceToPoint.distance - getDistance(point - e.yyx).distance);

    return normalize(n);
}

CollisionInfo rayMarch(vec3 rayOrigin, vec3 rayDirection)
{
    CollisionInfo distanceToOrigin;

    for (int i = 0; i < MAX_STEPS; i++)
    {
        vec3 point = rayOrigin + rayDirection * distanceToOrigin.distance;
        CollisionInfo distanceToScene = getDistance(point);

        distanceToOrigin.distance += distanceToScene.distance;
        distanceToOrigin.color = distanceToScene.color;
        if (distanceToOrigin.distance > MAX_DIST || distanceToScene.distance < SURF_DIST)
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

    CollisionInfo d = rayMarch(point + n * SURF_DIST * 5., lightPosNormalized);

    if (d.distance < length(lightPosition - point))
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

    CollisionInfo colInfo = rayMarch(camera.position, camera.direction);

    vec4 col = vec4(0);

    if (colInfo.distance < MAX_DIST)
    {
        vec3 collisionPoint = camera.position + camera.direction * colInfo.distance;

        col = vec4(vec3(getLight(collisionPoint)), 1.);

        col = colInfo.color * col;
    }

    fragColor = col;
}