# Презентация Ember Moth (видео 2–3 минуты)

Требования к видео: MP4-файл (не ссылка), **ваш голос, на английском**. Нужно рассказать: результат, основные шаги, проблемы и как вы их решили.
Ниже: что открыть на экране (по-русски) и **что говорить (по-английски, можно читать)**. Всего около 330 слов, это примерно 2,5 минуты спокойным темпом.

**Подготовка перед записью:** откройте Unity с проектом (сцена `Menu`), вкладку Game в режиме Device Simulator, на втором мониторе или во вкладке браузера репозиторий https://github.com/ars512/ember-moth-rk2 и файл `docs/GDD.pdf`. Закройте лишнее.

---

## 0:00–0:15 — Вступление
**Показать:** главное меню в Game View (16:9).

**Сказать:**
> Hi, I'm Ars. This is Ember Moth, a 2D mobile game for landscape phones. You play a night moth with a lantern: collect glowing embers and jump over dark shadows. In this video I show the result, the main steps, and the problems I solved.

## 0:15–1:05 — Результат: UI на разных экранах
**Показать:** в Device Simulator переключайте устройства: 16:9 телефон, iPhone (с вырезом), iPad (4:3).
На каждом: меню → нажать Play → HUD (сердца, счёт, пауза, стик, JUMP) → Pause → Resume. Не спешите, на каждый экран по 10–15 секунд.

**Сказать:**
> This is the main menu and the HUD on three screens: a 16:9 phone, a notch phone, and a 4:3 tablet. The Canvas Scaler is set to Scale With Screen Size, 1920 by 1080, match one. Every element is anchored: hearts top-left, score top-center, pause top-right, stick and jump button in the bottom corners. Buttons and text are inside a SafeArea panel, so nothing goes under the notch, but the background still fills the whole screen. Buttons are at least 130 pixels, text at least 56. Here is pause: time stops, Resume continues, Restart reloads, Menu goes back.

## 1:05–1:35 — Основные шаги (GDD и код)
**Показать:**
1. `docs/GDD.pdf` — пролистать две страницы (core loop, controls, art, MVP, cut list, plan).
2. В Unity, папка `Assets/Scripts` — выделить `SafeArea.cs`, открыть его.

**Сказать:**
> First I wrote the game design document: genre, core loop, controls, content, art style, minimum viable product, cut list, and a plan for weeks nine to thirteen. I work solo, so I'm the designer, programmer, and UI artist. The key script is SafeArea: it reads Screen.safeArea and sets the anchors of a panel, so the layout adapts to any notch.

## 1:35–2:10 — Проблемы и решения
**Показать:** пока говорите, снова переключите устройство на iPad в симуляторе и покажите меню.

**Сказать:**
> I had two main problems. First, on the tablet the game title was too wide and went off the screen, because with match set to height the width of the canvas is only 1440 units on 4:3. I made the title smaller and moved it. Second, the pause buttons were the same color as the panel, so they were hard to see. I made them a lighter blue. Also, the world must look right at any aspect ratio, so a small CameraFit script keeps the same visible width on phones and tablets.

## 2:10–2:40 — GitHub
**Показать:** страницу репозитория в браузере: файл `.gitignore`, отсутствие папки `Library`, `README.md`. Затем вкладка Commits (5+ коммитов) и вкладка Pull requests → закрытый (merged) PR #1.

**Сказать:**
> Here is my GitHub repository. It uses the Unity gitignore, so there is no Library folder. I made several commits with my own account, and a feature branch called feature slash ui-screens, which I merged into main with a pull request.

## 2:40–2:50 — Завершение
**Сказать:**
> That's all. Thank you for watching.

---

## Что загрузить в Canvas (чек-лист)
- [ ] `docs/GDD.pdf`
- [ ] Ссылка на репозиторий в комментарии к сдаче: https://github.com/ars512/ember-moth-rk2
- [ ] Скриншоты меню и HUD на **двух** размерах экрана (есть в `Screenshots/`: `menu_*` и `hud_*`, например 16:9 и tablet)
- [ ] Видео MP4 (запись экрана + голос, например OBS или Win+G)
- [ ] `AI_LOG.txt` лежит в репозитории

## Важное про ИИ (прочитайте до сдачи)
В критериях сказано: **ИИ-агенты, которые пишут код в проекте (Claude Code, Codex, Cursor Agent, Copilot agent), не разрешены**, а код из ИИ — максимум 90%. Этот проект целиком написан агентом Claude Code, и сейчас в `AI_LOG.txt` указан «ChatGPT», что неправда. Решение за вами: честнее всего исправить лог и обсудить с преподавателем, как быть. На защите в 14–15 неделях он может спросить про любую часть кода, так что код нужно прочитать и понимать.
