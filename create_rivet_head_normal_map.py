from PIL import Image
import numpy as np
import math


def create_normal_map(width, filename):
    # Create an empty image with RGB mode
    normal_map = Image.new('RGB', (width, width))

    radius = width // 4 # could be a parameter
    center = width // 2


    min_ny = 1.0
    max_ny = 0.0

    # Generate normal map data
    for y in range(width):
        for x in range(width):

            center_distance = math.sqrt((x - center) ** 2 + (y - center) ** 2)
            #if (x - center) ** 2 + (y - center) ** 2 > radius ** 2:
            if center_distance > radius:
                # Outside the circle, set normal to (0, 0, 1)
                nx = 0.0
                ny = 0.0
                nz = 1.0
            else:
                nx = (x - center) / radius
                ny = (center - y) / radius
                nz = math.sqrt(1.0 - ny * ny) # normalize the normal vector

            if ny < min_ny:
                min_ny = ny
            if ny > max_ny:
                max_ny = ny

            # Convert normal vector from range [-1, 1] to range [0.0, 1.0]
            nx = (nx + 1.0) * 0.5
            ny = (ny + 1.0) * 0.5
            nz = (nz + 1.0) * 0.5

            # Convert normal vector to RGB values
            r = int(nx * 255)
            g = int(ny * 255)
            b = int(nz * 255)

            # Set the pixel value
            normal_map.putpixel((x, y), (r, g, b))

    # Save the image as a PNG file
    normal_map.save(filename)
    print(f"Normal map saved to {filename}\nmin_ny: {min_ny}, max_ny: {max_ny}")

# Example usage
create_normal_map(32, 'rivet_normal_map.png')