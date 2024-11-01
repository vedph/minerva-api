# Attributes

This section lists all the text span attributes in Minerva, as defined by the combination of privileged and non-privileged attributes.

>Structures in Minerva are `p` whose value is characters count; `snt` whose value is characters count; `fp-lat` whose value is the phrase text.

## Document

- privileged:
  - `author`: always equal to `-`.
  - `title`: the file name of the source document, without extension.
  - `date_value`: the year extracted from the document's date.
  - `sort_key`: a sort key built by the default builder from author, title, and date.
  - `source`: the full path of the source document.
  - `profile_id`: always equal to `atti-chiari`.
- non-privileged:
  - `atto`: category.
  - `data`: date in the form YYYYMMDD. This is also the source for document's `date_value` which just contains the year.
  - `giudicante`: tribunal name.
  - `grado`: 1-3.
  - `gruppo-atto`: group tag (excluded for split documents).
  - `gruppo-nr`: number (excluded for split documents).
  - `materia`: topic (like "civile", "penale", etc.).
  - `nascita-avv`: year.
  - `sede-giudicante`: city name.
  - `sede-raccolta`: one of `fi`, `ge`, `le`, `vt`.
  - `sesso-avv`: one of `F`, `M`, `X`.

## Span

- privileged:
  - `p1`
  - `p2`
  - `index`
  - `length`
  - `language`: null for Italian, else ISO369-3 (`eng`, `fra`, `lat`).
  - `pos`: `ADJ` `ADP` `ADV` `AUX` `CCONJ` `DET` `NOUN` `NUM` `PRON` `PROPN` `SCONJ` `SYM` `VERB` `X`.
  - `lemma`
  - `value`
  - `text`
- non-privileged:
  - `abbr`
  - `address`
  - `b`: bold (`1`).
  - `clitic`
  - `definite`
  - `degree`
  - `deprel`
  - `email`
  - `foreign`: ISO639-3 code.
  - `gender`: `masc`, `fem`.
  - `i`: italic (`1`).
  - `lemma`
  - `len`: letters count.
  - `lp`: left punctuation.
  - `misc`
  - `mood`
  - `n`: digits (`1`).
  - `number`: `sing`, `plur`.
  - `numtype`: `ord`, `card`.
  - `org-f`: female juridic person name.
  - `org-m`: male juridic person name.
  - `person`: 1-3.
  - `pn-f`: feminine person name.
  - `pn-m`: masculine person name.
  - `pn-s`: surname.
  - `poss`: `yes`.
  - `prontype`: pronoun type: `art`, `dem`, `ind`, `int`, `neg`, `prs`, `rel`, `tot`.
  - `rp`: right punctuation.
  - `tense`: `fut`, `imp`, `past`, `pres`.
  - `verbform`: `inf`, `ger`, `part`, `fn`.
