# README #

This is library for parsing Clausewitz Engine savegame files mainly for use with Chronicle program. It is able to parse text files autonomously and binary ('Ironman') files in cooperation with CE.Ironmelt library.

File parsing results in creating a logical tree of nodes that can be walked through in parent-child relationships and quickly traversed to pull out necessary information.

I want to concentrate further development on optimizing the process to differentiate whether the tree should be parsed fully (for binary-to-text conversion) or only partially to limit resource usage.