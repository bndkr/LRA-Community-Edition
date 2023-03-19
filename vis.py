import matplotlib.pyplot as plt;
import numpy as np;
from math import atan;

all_data = np.loadtxt("report.csv", skiprows=1, delimiter=',')

timesteps = all_data[:,0]
xpos = all_data[:,1]
ypos = all_data[:,2]
xvel = all_data[:,3]
yvel = all_data[:,4]
speed = all_data[:,5]
xacc = all_data[:,6]
yacc = all_data[:,7]
accmag = all_data[:,8]
freefall = all_data[:,9]

dir_array = []

for i in range(xvel.shape[0]):
    dir_array.append(atan(yvel[i] / xvel[i]))

directions = np.array(dir_array)

plt.figure("speed histogram")
plt.hist(ypos, bins=40)

plt.figure("position")
plt.plot(xpos, ypos)

# plt.figure("velocities")
# plt.plot(xacc, yacc)

# plt.figure("acceleration magnitude")
# plt.plot(timesteps, accmag)

plt.figure("speed over time")
plt.plot(timesteps, speed)

# plt.figure("direction over time")
# plt.plot(timesteps, directions)

plt.show()

