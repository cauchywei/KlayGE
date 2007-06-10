#ifndef _CONFIG_HPP
#define _CONFIG_HPP

#if !defined(__cplusplus)
	#error C++ compiler required.
#endif

#if defined(DEBUG) | defined(_DEBUG)
    #define KLAYGE_DEBUG
#endif

// Defines supported compilers
#if defined(__GNUC__)
	// GNU C++

	#define KLAYGE_COMPILER_GCC

	#if __GNUC__ >= 4
		#define KLAYGE_COMPILER_VERSION 4.0
	#elif __GNUC__ >= 3
		#define KLAYGE_COMPILER_VERSION 3.0
	#else
		#error Unknown compiler.
	#endif
#elif defined(_MSC_VER)
	#define KLAYGE_COMPILER_MSVC

	#if _MSC_VER >= 1400
		#define KLAYGE_COMPILER_VERSION 8.0
		#pragma warning(disable: 4819)
	#elif _MSC_VER >= 1310
		#define KLAYGE_COMPILER_VERSION 7.1
	#else
		#error Unknown compiler.
	#endif
#endif

// Defines supported platforms
#if defined(_XBOX_VER)
	#if _XBOX_VER >= 200
		#define KLAYGE_PLATFORM_XBOX360
	#else
		#error Unknown platform.
	#endif
#elif defined(_WIN32) || defined(__WIN32__) || defined(WIN32)
	#define KLAYGE_PLATFORM_WINDOWS

	#if defined(_WIN64)
		#define KLAYGE_PLATFORM_WIN64
	#else
		#define KLAYGE_PLATFORM_WIN32
	#endif

	// Forces all boost's libraries to be linked as dll
	#define BOOST_ALL_DYN_LINK

	// Shut min/max in windows.h
	#define NOMINMAX

	#if defined(__MINGW32__)
		#include <_mingw.h>
	#endif
#elif defined(__CYGWIN__)
	#define KLAYGE_PLATFORM_CYGWIN
#elif defined(linux) || defined(__linux) || defined(__linux__)
	#define KLAYGE_PLATFORM_LINUX
#else
	#error Unknown platform.
#endif

// Defines supported CPUs
#if defined(KLAYGE_COMPILER_MSVC)
	#if defined(_M_X64)
		#define KLAYGE_CPU_X64
	#elif defined(_M_IX86)
		#define KLAYGE_CPU_X86
	#else
		#error Unknown CPU type.
	#endif
#elif defined(KLAYGE_COMPILER_GCC)
	#if defined(__x86_64__)
		#define KLAYGE_CPU_X64
	#elif defined(__i386__)
		#define KLAYGE_CPU_X86
	#else
		#error Unknown CPU type.
	#endif
#endif

// Defines the native endian
#if defined(KLAYGE_CPU_X86) || defined(KLAYGE_CPU_X64) || defined(KLAYGE_PLATFORM_WINDOWS)
	#define KLAYGE_LITTLE_ENDIAN
#else
	#define KLAYGE_BIG_ENDIAN
#endif

// Defines some MACRO from compile options
#define _IDENTITY_SUPPORT
#define _SELECT1ST2ND_SUPPORT
#define _PROJECT1ST2ND_SUPPORT
#define _COPYIF_SUPPORT

#endif		// _CONFIG_HPP
