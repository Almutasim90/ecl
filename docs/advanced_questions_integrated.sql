-- ============================================================================
-- ADVANCED GRAMMAR QUESTIONS — INTEGRATED INTO UNIFIED SCHEMA
-- Topic 15: Inversions & Emphasis  (30 questions: 12 Beg / 12 Int / 6 Adv)
--
-- This is the original docs/advanced questions .sql converted to fit the
-- existing public.grammarquestions table used by the ECL application:
--   - topic              -> grammartype          (text)
--   - level              -> level                (text: Beginner/Intermediate/Advanced)
--   - question_text      -> questiontext         (text)
--   - option_a..option_d -> optiona..optiond     (text)
--   - correct_answer A-D -> correctoption 1-4    (integer)
--   - explanation        -> explanation          (text)
--
-- Apply once to the DB after running EF migrations (which add level + explanation).
-- Safe to re-run only if you first delete the rows below; INSERT is not idempotent.
-- ============================================================================

INSERT INTO public.grammarquestions
    (grammartype, level, questiontext, optiona, optionb, optionc, optiond, correctoption, explanation)
VALUES

-- ========== BEGINNER LEVEL (12 Questions) ==========

-- Basic Inversion in Questions (4 questions)
('Inversions & Emphasis', 'Beginner', '_____ you like pizza?', 'Do', 'Does', 'Is', 'Are', 1, 'Basic question inversion: auxiliary + subject.'),
('Inversions & Emphasis', 'Beginner', 'Where _____ she live?', 'do', 'does', 'is', 'are', 2, 'Question word + auxiliary + subject.'),
('Inversions & Emphasis', 'Beginner', '_____ they coming to the party?', 'Do', 'Does', 'Is', 'Are', 4, 'Are + they in question.'),
('Inversions & Emphasis', 'Beginner', 'Why _____ you cry?', 'do', 'does', 'are', 'is', 1, 'Why + do + you.'),

-- "So" and "Neither/Nor" for Agreement (4 questions)
('Inversions & Emphasis', 'Beginner', 'I like coffee. _____ do I.', 'So', 'Neither', 'Nor', 'Also', 1, 'So + auxiliary + subject for agreement with positive.'),
('Inversions & Emphasis', 'Beginner', 'I don''t like coffee. _____ do I.', 'So', 'Neither', 'Not', 'Also', 2, 'Neither/Nor + auxiliary + subject for negative agreement.'),
('Inversions & Emphasis', 'Beginner', 'She can swim. _____ can he.', 'So', 'Neither', 'Nor', 'Also', 1, 'So + modal + subject.'),
('Inversions & Emphasis', 'Beginner', 'He hasn''t called. _____ have they.', 'So', 'Neither', 'Either', 'Also', 2, 'Neither + auxiliary + subject.'),

-- Emphatic "do/does/did" (4 questions)
('Inversions & Emphasis', 'Beginner', 'I _____ like you. You are wrong about me.', 'do', 'does', 'did', 'doing', 1, 'Emphatic do in present tense for contradiction/emphasis.'),
('Inversions & Emphasis', 'Beginner', 'She _____ want to help. She said so.', 'do', 'does', 'is', 'are', 2, 'Emphatic does + base verb (does want, not wants).'),
('Inversions & Emphasis', 'Beginner', 'I _____ see him yesterday. I swear!', 'do', 'does', 'did', 'was', 3, 'Emphatic did + base verb for past.'),
('Inversions & Emphasis', 'Beginner', 'They _____ try their best, even if they failed.', 'do', 'did', 'are', 'were', 2, 'Emphatic did for past emphasis.'),

-- ========== INTERMEDIATE LEVEL (12 Questions) ==========

-- Negative Inversions (Never, Rarely, Seldom, Little) (4 questions)
('Inversions & Emphasis', 'Intermediate', 'Never _____ I seen such a beautiful sunset.', 'have', 'has', 'had', 'did', 1, 'Never + auxiliary + subject + V3 (present perfect).'),
('Inversions & Emphasis', 'Intermediate', 'Rarely _____ they arrive on time.', 'do', 'does', 'are', 'have', 1, 'Rarely + do + subject + base verb.'),
('Inversions & Emphasis', 'Intermediate', 'Seldom _____ we eat out.', 'do', 'does', 'are', 'have', 1, 'Seldom + auxiliary + subject.'),
('Inversions & Emphasis', 'Intermediate', 'Little _____ she know what was coming.', 'did', 'does', 'had', 'has', 1, 'Little = not at all, inversion with did.'),

-- Inversions with "No sooner", "Hardly", "Scarcely" (4 questions)
('Inversions & Emphasis', 'Intermediate', 'No sooner _____ I arrived than the phone rang.', 'had', 'have', 'did', 'was', 1, 'No sooner + past perfect + than + past simple.'),
('Inversions & Emphasis', 'Intermediate', 'Hardly _____ I started when it began to rain.', 'had', 'have', 'did', 'was', 1, 'Hardly + past perfect + when.'),
('Inversions & Emphasis', 'Intermediate', 'Scarcely _____ we left when the storm hit.', 'had', 'have', 'did', 'were', 1, 'Scarcely + past perfect.'),
('Inversions & Emphasis', 'Intermediate', 'No sooner _____ we sit down than the fire alarm went off.', 'had', 'have', 'did', 'was', 3, 'No sooner + past simple (with did) is also common; past perfect more formal.'),

-- Inversions with "Only" (Only after, only then, only when, only if, only by) (4 questions)
('Inversions & Emphasis', 'Intermediate', 'Only after _____ the letter did I understand the truth.', 'I read', 'did I read', 'had I read', 'I had read', 1, 'Only after + subject + verb + inversion in main clause.'),
('Inversions & Emphasis', 'Intermediate', 'Only then _____ I realize my mistake.', 'did', 'had', 'have', 'was', 1, 'Only then + did + subject.'),
('Inversions & Emphasis', 'Intermediate', 'Only by _____ hard can you succeed.', 'work', 'working', 'to work', 'worked', 2, 'Only by + gerund + inversion.'),
('Inversions & Emphasis', 'Intermediate', 'Only if you _____ will I come.', 'apologize', 'do apologize', 'apologized', 'apologizing', 1, 'Only if + subject + verb + inversion in main.'),

-- ========== ADVANCED LEVEL (6 Questions) ==========

-- Inversions with "Not only...but also" (2 questions)
('Inversions & Emphasis', 'Advanced', 'Not only _____ she late, but she also forgot her keys.', 'was', 'is', 'were', 'had', 1, 'Not only + auxiliary + subject + but also.'),
('Inversions & Emphasis', 'Advanced', 'Not only _____ he speak English, but he also speaks French.', 'does', 'do', 'is', 'can', 1, 'Not only + does + subject + base verb.'),

-- Inversions with "Such" and "So" (2 questions)
('Inversions & Emphasis', 'Advanced', 'Such _____ the beauty of the place that I was speechless.', 'was', 'is', 'were', 'has', 1, 'Such + be + subject + that.'),
('Inversions & Emphasis', 'Advanced', 'So beautiful _____ the sunset that we stopped to watch.', 'was', 'is', 'were', 'did', 1, 'So + adjective + be + subject.'),

-- Cleft Sentences for Emphasis (It is/was...that; What...is) (2 questions)
('Inversions & Emphasis', 'Advanced', 'It was John _____ broke the vase.', 'who', 'that', 'which', 'both A and B', 4, 'It-cleft: It was + emphasized element + that/who.'),
('Inversions & Emphasis', 'Advanced', 'What I need _____ a break.', 'is', 'are', 'was', 'were', 1, 'What-clause + be + emphasized element.');

-- ========== END OF TOPIC 15 ==========
