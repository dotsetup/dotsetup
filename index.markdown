---
layout: home
title: Home
nav_order: 1
---

# An open source Windows installer that works.

dotSetup is a <b>free and open source</b> installer for Windows programs. It is built to be a modern alternative to older installers often not optimized for the latest Windows OS versions. dotSetup is  designed to be as small and flexible as possible and is therefore very suitable for internet distribution. dotSetup is written in .NET and available under the <b>GPLv3</b> <a href="/license">license</a>.

[Get dotSetup](https://github.com/dotsetup/dotsetup/){: .btn .btn-primary .mr-5 } [Read Docs](https://github.com/dotsetup/dotsetup/wiki){: .btn }

{% include carousel.html height="400" unit="px" duration="7" %}

dotSetup has taken a unique approach and is available as both a complete standalone installer or a library. This approach allows ease of use on one hand and at the same time flexibility for having solutions to any real-life scenarios. The standalone installer is powerful enough to take care of the majority of installation flows, however if some features are missing it is always possible to alter the source code of the standalone installer or develop a completely new solution using the dotSetup library. The code is written in C# and the development is fairly easy using Microsoft Visual Studio.

<div class="features">
    <div>
      <i class="fas fa-cog" aria-hidden="true"></i>
      <div class="features-title">64-bit support</div>
      <div class="features-description">Supports both 32-bit and 64-bit applications</div>
    </div>
    <div>
      <i class="fab fa-windows" aria-hidden="true"></i>
      <div class="features-title">Legacy Windows OS</div>
      <div class="features-description">Windows XP SP2 and higher support</div>
    </div>
    <div>
      <i class="fas fa-user-cog" aria-hidden="true"></i>
      <div class="features-title">Non-administrative installations</div>
      <div class="features-description">Support for administrative/non-administrative installations</div>
    </div>
    <div>
      <i class="fas fa-trash" aria-hidden="true"></i>
      <div class="features-title">Uninstall</div>
      <div class="features-description">Complete Uninstall handling</div>
    </div>
    <div>
      <i class="fas fa-compress-arrows-alt" aria-hidden="true"></i>
      <div class="features-title">Compression</div>
      <div class="features-description">Built-in compression support</div>
    </div>
    <div>
      <i class="far fa-window-maximize" aria-hidden="true"></i>
      <div class="features-title">Generic and Custom UI</div>
      <div class="features-description">Customizable and Standard Windows wizard interface</div>
    </div>
    <div>
      <i class="far fa-bell-slash" aria-hidden="true"></i>
      <div class="features-title">Silent Install</div>
      <div class="features-description">Silent install and uninstall</div>
    </div>
    <div>
      <i class="fas fa-globe-americas" aria-hidden="true"></i>
      <div class="features-title">Unicode</div>
      <div class="features-description">Full Unicode support</div>
    </div>
    <div>
      <i class="fas fa-arrow-alt-circle-down" aria-hidden="true"></i>
      <div class="features-title">Tiny footprint</div>
      <div class="features-description">Tiny footprint both in binary size and memory usage</div>
    </div>
    <div>
      <i class="fas fa-download" aria-hidden="true"></i>
      <div class="features-title">BITS</div>
      <div class="features-description">Microsoft Background Intelligent Transfer Service for uninterrupted download</div>
    </div>
</div>

In addition, dotSetup can:

<ul>
  <li>Ability to register DLL’s/OCX’s</li>
  <li>Creation of shortcuts everywhere</li>
  <li>Execution of other processes during various installation stages</li>
  <li>Reading and writing to Windows Registry</li>
  <li>Requires .NET Version 4.0 and up</li>
</ul>

If you’ve added some new cool features to the installer and feel like sharing it’s also very easy – just open a pull request at our github account. dotSetup is a true open source and created by (and for) developers like you.
