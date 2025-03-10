from PIL import Image
import numpy as np
import math

def exponential_decay(x: float, k: float, x_multiplier: float, x_offset: float) -> float:
    """
    Computes the value of the exponential decay function y = e^(-kx).
    
    Parameters:
    x (float): Input value for x [0-1].
    k (float): Decay rate.
    
    Returns:
    float: Computed value of y.
    """
    return math.exp(-k * (x + x_offset) * x_multiplier)

def create_normal_map(width, height, filename):
    # Create an empty image with RGB mode
    normal_map = Image.new('RGB', (width, height))

    # Define the number of horizontal areas
    num_areas = 6
    area_height = height // num_areas

    # Calculate the centers of all areas
    area_centers = [(i + 0.5) * area_height for i in range(num_areas)]

    # Define the width of the flat area around each center
    flat_area_width = area_height * 0.1
    flattening_factor = 2.0

    # Generate normal map data
    for y in range(height):
        for x in range(width):
            # Example: Create a simple normal map with normals pointing up
            nx = 0.0  # X component of the normal (-1 to 1)
            ny = 0.0  # Y component of the normal (-1 to 1)
            nz = 1.0  # Z component of the normal (-1 to 1)

            # Find the nearest area center
            nearest_center = min(area_centers, key=lambda center: abs(y - center))

            if abs(y - nearest_center) < flat_area_width:
                ny = 0.0  # Flat area near the center
            else:
                if y < nearest_center: # above the center
                    d = nearest_center - y - flat_area_width
                    d_relative = d / (area_height / 2)
                    ny = exponential_decay(d_relative, 2, 1.8, 0.2)
                else: # below the center
                    d = y - nearest_center - flat_area_width
                    d_relative = d / (area_height / 2)
                    ny = - exponential_decay(d_relative, 2, 1.8, 0.2)
                nz = math.sqrt(1.0 - ny * ny) # normalize the normal vector

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

# Example usage
create_normal_map(512, 128, 'normal_map.png')