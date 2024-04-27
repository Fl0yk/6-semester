# Additional clean files
cmake_minimum_required(VERSION 3.16)

if("${CONFIG}" STREQUAL "" OR "${CONFIG}" STREQUAL "Debug")
  file(REMOVE_RECURSE
  "CMakeFiles\\DeadPixel_autogen.dir\\AutogenUsed.txt"
  "CMakeFiles\\DeadPixel_autogen.dir\\ParseCache.txt"
  "DeadPixel_autogen"
  )
endif()
