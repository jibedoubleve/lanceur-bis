---
layout: page
title: Releases
nav_order: 1
---

# Releases

<ul>
  {% for release in site.github.releases %}
    {% unless release.prerelease %}
      <li>
        <h2><a href="{{ release.html_url }}">Release of version {{ release.name }}</a></h2>
        <p>{{ release.body | markdownify }}</p>
      </li>
    {% endunless %}
  {% endfor %}
</ul>