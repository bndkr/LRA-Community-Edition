import matplotlib.pyplot as plt;
import numpy as np;

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

plt.figure()
plt.plot(xacc, yacc)
plt.show()

