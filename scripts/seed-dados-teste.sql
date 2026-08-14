/* =============================================================================
   PSG — dados de teste para conferir as telas com conteúdo real.

   Como usar: rode este script no banco da aplicação DEPOIS do update-database
   (SSMS, Azure Data Studio ou `sqlcmd -S <servidor> -d <banco> -i seed-dados-teste.sql`).

   O script é re-executável: antes de inserir, ele apaga SÓ o que ele mesmo cria
   (os cursos listados abaixo, os módulos e alunos deles, as inscrições
   correspondentes e os professores com matrícula 'SEED-P%'). Nenhuma outra
   linha do banco é tocada.

   As datas são relativas ao dia da execução (DATEADD sobre GETDATE()), então o
   gráfico de linha e o card de cancelamentos recentes do dashboard continuam
   fazendo sentido independentemente de quando o script for rodado.

   O que os dados cobrem:
     - 5 cursos, sendo um sem módulo e sem aluno (estados vazios das telas)
     - um curso com 12 módulos (rolagem da legenda do gráfico e da lista)
     - 45 alunos: Cursando, Finalizado e Em espera (paginação de 20 por página)
     - reprovações com nota, cancelamentos recentes, um aluno sem matrícula
     - inscrições com e sem data de matrícula e sem data de fim
   ============================================================================= */

SET NOCOUNT ON;

DECLARE @hoje DATE = CAST(GETDATE() AS DATE);

/* ---------------------------------------------------------------------------
   1) Limpeza do que este script criou em execuções anteriores
   --------------------------------------------------------------------------- */
DECLARE @cursosSeed TABLE (IdCurso INT);
INSERT INTO @cursosSeed (IdCurso)
SELECT IdCurso FROM Cursos WHERE Nome IN (N'Desenvolvimento de Sistemas',
       N'Administração',
       N'Enfermagem do Trabalho',
       N'Logística',
       N'Recursos Humanos');

DELETE FROM AlunoModulo
WHERE IdAluno  IN (SELECT IdAluno  FROM Alunos  WHERE IdCurso IN (SELECT IdCurso FROM @cursosSeed))
   OR IdModulo IN (SELECT IdModulo FROM Modulos WHERE IdCurso IN (SELECT IdCurso FROM @cursosSeed));

DELETE FROM Alunos  WHERE IdCurso IN (SELECT IdCurso FROM @cursosSeed);
DELETE FROM Modulos WHERE IdCurso IN (SELECT IdCurso FROM @cursosSeed);
DELETE FROM Cursos  WHERE IdCurso IN (SELECT IdCurso FROM @cursosSeed);
DELETE FROM Professores WHERE Matricula LIKE 'SEED-P%';

/* ---------------------------------------------------------------------------
   2) Professores
   --------------------------------------------------------------------------- */
DECLARE @professores TABLE (IdProfessor INT, Matricula NVARCHAR(50));

INSERT INTO Professores (Matricula, Nome)
OUTPUT inserted.IdProfessor, inserted.Matricula INTO @professores (IdProfessor, Matricula)
VALUES
    (N'SEED-P001', N'Marcos Vinícius Andrade'),
    (N'SEED-P002', N'Renata Lopes Ferreira'),
    (N'SEED-P003', N'Paulo Henrique Braga'),
    (N'SEED-P004', N'Juliana Castro Mendes'),
    (N'SEED-P005', N'Eduardo Tavares Pinto');

/* ---------------------------------------------------------------------------
   3) Cursos (cada um com seu coordenador)
   --------------------------------------------------------------------------- */
DECLARE @cursos TABLE (IdCurso INT, Nome NVARCHAR(100));

INSERT INTO Cursos (Nome, DataCadastro, Status, IdCoordenador)
OUTPUT inserted.IdCurso, inserted.Nome INTO @cursos (IdCurso, Nome)
SELECT v.Nome, DATEADD(DAY, v.Dias, @hoje), 1, p.IdProfessor
FROM (VALUES
    (CAST(N'Desenvolvimento de Sistemas' AS NVARCHAR(100)), -540, CAST(N'SEED-P001' AS NVARCHAR(50))),
    (N'Administração', -500, N'SEED-P002'),
    (N'Enfermagem do Trabalho', -470, N'SEED-P003'),
    (N'Logística', -300, N'SEED-P004'),
    (N'Recursos Humanos', -60, N'SEED-P005')
    ) AS v(Nome, Dias, MatriculaCoordenador)
JOIN @professores p ON p.Matricula = v.MatriculaCoordenador;

/* ---------------------------------------------------------------------------
   4) Módulos
   --------------------------------------------------------------------------- */
DECLARE @modulos TABLE (IdModulo INT, IdCurso INT, Numero INT);

INSERT INTO Modulos (IdCurso, Nome, Numero, DataCadastro, Status, IdProfessor)
OUTPUT inserted.IdModulo, inserted.IdCurso, inserted.Numero INTO @modulos (IdModulo, IdCurso, Numero)
SELECT c.IdCurso, v.Nome, v.Numero, DATEADD(DAY, v.Dias, @hoje), 1, p.IdProfessor
FROM (VALUES
    (CAST(N'Desenvolvimento de Sistemas' AS NVARCHAR(100)), CAST(N'Lógica de Programação' AS NVARCHAR(100)), 1, -519, CAST(N'SEED-P001' AS NVARCHAR(50))),
    (N'Desenvolvimento de Sistemas', N'Banco de Dados', 2, -518, N'SEED-P003'),
    (N'Desenvolvimento de Sistemas', N'Front-end com HTML e CSS', 3, -517, N'SEED-P005'),
    (N'Desenvolvimento de Sistemas', N'JavaScript Essencial', 4, -516, N'SEED-P001'),
    (N'Desenvolvimento de Sistemas', N'Programação em C#', 5, -515, N'SEED-P003'),
    (N'Desenvolvimento de Sistemas', N'Orientação a Objetos', 6, -514, N'SEED-P005'),
    (N'Desenvolvimento de Sistemas', N'ASP.NET Core', 7, -513, N'SEED-P001'),
    (N'Desenvolvimento de Sistemas', N'Entity Framework', 8, -512, N'SEED-P003'),
    (N'Desenvolvimento de Sistemas', N'Arquitetura de Software', 9, -511, N'SEED-P005'),
    (N'Desenvolvimento de Sistemas', N'Testes Automatizados', 10, -510, N'SEED-P001'),
    (N'Desenvolvimento de Sistemas', N'DevOps e Cloud', 11, -509, N'SEED-P003'),
    (N'Desenvolvimento de Sistemas', N'Projeto Integrador', 12, -508, N'SEED-P005'),
    (N'Administração', N'Fundamentos da Administração', 1, -479, N'SEED-P002'),
    (N'Administração', N'Contabilidade Gerencial', 2, -478, N'SEED-P004'),
    (N'Administração', N'Gestão de Pessoas', 3, -477, N'SEED-P002'),
    (N'Administração', N'Marketing Estratégico', 4, -476, N'SEED-P004'),
    (N'Administração', N'Finanças Corporativas', 5, -475, N'SEED-P002'),
    (N'Administração', N'Planejamento Estratégico', 6, -474, N'SEED-P004'),
    (N'Enfermagem do Trabalho', N'Saúde Ocupacional', 1, -449, N'SEED-P003'),
    (N'Enfermagem do Trabalho', N'Ergonomia Aplicada', 2, -448, N'SEED-P001'),
    (N'Enfermagem do Trabalho', N'Primeiros Socorros', 3, -447, N'SEED-P003'),
    (N'Enfermagem do Trabalho', N'Legislação em Saúde', 4, -446, N'SEED-P001'),
    (N'Enfermagem do Trabalho', N'Gestão de Riscos', 5, -445, N'SEED-P003'),
    (N'Logística', N'Cadeia de Suprimentos', 1, -279, N'SEED-P004'),
    (N'Logística', N'Gestão de Estoques', 2, -278, N'SEED-P002'),
    (N'Logística', N'Transporte e Distribuição', 3, -277, N'SEED-P004')
    ) AS v(NomeCurso, Nome, Numero, Dias, MatriculaProfessor)
JOIN @cursos c      ON c.Nome = v.NomeCurso
JOIN @professores p ON p.Matricula = v.MatriculaProfessor;

/* ---------------------------------------------------------------------------
   5) Alunos
   DataCadastro espalhada nos últimos ~14 meses: é ela que alimenta o gráfico
   de linha do dashboard.
   --------------------------------------------------------------------------- */
DECLARE @alunos TABLE (IdAluno INT, IdCurso INT, Nome NVARCHAR(100));

INSERT INTO Alunos (IdCurso, Matricula, Nome, Celular, DataCadastro, Status)
OUTPUT inserted.IdAluno, inserted.IdCurso, inserted.Nome INTO @alunos (IdAluno, IdCurso, Nome)
SELECT c.IdCurso, v.Matricula, v.Nome, v.Celular, DATEADD(DAY, v.Dias, @hoje), 1
FROM (VALUES
    (CAST(N'Desenvolvimento de Sistemas' AS NVARCHAR(100)), CAST(N'SEED-A001' AS NVARCHAR(20)), CAST(N'Ana Maria Silva Rodrigues' AS NVARCHAR(100)), CAST(NULL AS NVARCHAR(20)), -1),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A002' AS NVARCHAR(20)), N'Carlos Eduardo Nascimento', CAST(N'11922877244' AS NVARCHAR(20)), -32),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A003' AS NVARCHAR(20)), N'Beatriz Antunes Moreira', CAST(NULL AS NVARCHAR(20)), -62),
    (N'Desenvolvimento de Sistemas', CAST(NULL AS NVARCHAR(20)), N'João Pedro de Almeida Costa', CAST(NULL AS NVARCHAR(20)), -96),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A005' AS NVARCHAR(20)), N'Mariana Souza Lima', CAST(N'11944108462' AS NVARCHAR(20)), -125),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A006' AS NVARCHAR(20)), N'Rafael Oliveira Duarte', CAST(N'11954579177' AS NVARCHAR(20)), -153),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A007' AS NVARCHAR(20)), N'Camila Fernandes Rocha', CAST(N'11959172939' AS NVARCHAR(20)), -35),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A008' AS NVARCHAR(20)), N'Bruno Henrique Martins', CAST(NULL AS NVARCHAR(20)), -216),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A009' AS NVARCHAR(20)), N'Larissa Gomes Pereira', CAST(NULL AS NVARCHAR(20)), -162),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A010' AS NVARCHAR(20)), N'Thiago Ribeiro Cardoso', CAST(N'11947837135' AS NVARCHAR(20)), -389),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A011' AS NVARCHAR(20)), N'Juliana Barbosa Freitas', CAST(NULL AS NVARCHAR(20)), -210),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A012' AS NVARCHAR(20)), N'Felipe Augusto Ramos', CAST(NULL AS NVARCHAR(20)), -199),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A013' AS NVARCHAR(20)), N'Patrícia Nunes Teixeira', CAST(N'11990021315' AS NVARCHAR(20)), -324),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A014' AS NVARCHAR(20)), N'Gustavo Henrique Lopes', CAST(N'11971891684' AS NVARCHAR(20)), -65),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A015' AS NVARCHAR(20)), N'Amanda Cristina Vieira', CAST(N'11980866531' AS NVARCHAR(20)), -71),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A016' AS NVARCHAR(20)), N'Rodrigo Menezes Barros', CAST(N'11996864559' AS NVARCHAR(20)), -305),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A017' AS NVARCHAR(20)), N'Letícia Carvalho Pinto', CAST(N'11962987339' AS NVARCHAR(20)), -241),
    (N'Desenvolvimento de Sistemas', CAST(N'SEED-A018' AS NVARCHAR(20)), N'Vinícius Moraes Cunha', CAST(N'11918438691' AS NVARCHAR(20)), -131),
    (N'Administração', CAST(N'SEED-A019' AS NVARCHAR(20)), N'Fernanda Alves Machado', CAST(N'11913838669' AS NVARCHAR(20)), -213),
    (N'Administração', CAST(N'SEED-A020' AS NVARCHAR(20)), N'Marcelo Tadeu Siqueira', CAST(NULL AS NVARCHAR(20)), -395),
    (N'Administração', CAST(N'SEED-A021' AS NVARCHAR(20)), N'Isabela Monteiro Dias', CAST(NULL AS NVARCHAR(20)), -229),
    (N'Administração', CAST(N'SEED-A022' AS NVARCHAR(20)), N'Leonardo Farias Correia', CAST(N'11925120908' AS NVARCHAR(20)), -175),
    (N'Administração', CAST(N'SEED-A023' AS NVARCHAR(20)), N'Natália Prado Bezerra', CAST(N'11989238447' AS NVARCHAR(20)), -391),
    (N'Administração', CAST(N'SEED-A024' AS NVARCHAR(20)), N'Diego Antunes Cavalcanti', CAST(NULL AS NVARCHAR(20)), -388),
    (N'Administração', CAST(N'SEED-A025' AS NVARCHAR(20)), N'Priscila Ramos Andrade', CAST(NULL AS NVARCHAR(20)), -221),
    (N'Administração', CAST(N'SEED-A026' AS NVARCHAR(20)), N'André Luiz Sampaio', CAST(NULL AS NVARCHAR(20)), -114),
    (N'Administração', CAST(N'SEED-A027' AS NVARCHAR(20)), N'Vanessa Coelho Braga', CAST(NULL AS NVARCHAR(20)), -255),
    (N'Administração', CAST(N'SEED-A028' AS NVARCHAR(20)), N'Eduardo Pacheco Nogueira', CAST(NULL AS NVARCHAR(20)), -343),
    (N'Administração', CAST(N'SEED-A029' AS NVARCHAR(20)), N'Aline Ferreira Batista', CAST(N'11965492373' AS NVARCHAR(20)), -321),
    (N'Administração', CAST(N'SEED-A030' AS NVARCHAR(20)), N'Ricardo Salles Tavares', CAST(N'11932988467' AS NVARCHAR(20)), -390),
    (N'Enfermagem do Trabalho', CAST(N'SEED-A031' AS NVARCHAR(20)), N'Tatiane Miranda Rezende', CAST(N'11962854804' AS NVARCHAR(20)), -210),
    (N'Enfermagem do Trabalho', CAST(N'SEED-A032' AS NVARCHAR(20)), N'Paulo Sérgio Fonseca', CAST(N'11955452963' AS NVARCHAR(20)), -264),
    (N'Enfermagem do Trabalho', CAST(N'SEED-A033' AS NVARCHAR(20)), N'Débora Cristina Aguiar', CAST(NULL AS NVARCHAR(20)), -304),
    (N'Enfermagem do Trabalho', CAST(N'SEED-A034' AS NVARCHAR(20)), N'Wesley Cardoso Pontes', CAST(N'11974704240' AS NVARCHAR(20)), -71),
    (N'Enfermagem do Trabalho', CAST(N'SEED-A035' AS NVARCHAR(20)), N'Renata Aparecida Lima', CAST(NULL AS NVARCHAR(20)), -218),
    (N'Enfermagem do Trabalho', CAST(N'SEED-A036' AS NVARCHAR(20)), N'Fábio Junqueira Neves', CAST(NULL AS NVARCHAR(20)), -115),
    (N'Enfermagem do Trabalho', CAST(N'SEED-A037' AS NVARCHAR(20)), N'Simone Rocha Guimarães', CAST(N'11958115107' AS NVARCHAR(20)), -323),
    (N'Enfermagem do Trabalho', CAST(N'SEED-A038' AS NVARCHAR(20)), N'Alexandre Pires Domingues', CAST(N'11919813506' AS NVARCHAR(20)), -75),
    (N'Enfermagem do Trabalho', CAST(N'SEED-A039' AS NVARCHAR(20)), N'Bianca Toledo Amaral', CAST(N'11950391791' AS NVARCHAR(20)), -196),
    (N'Logística', CAST(N'SEED-A040' AS NVARCHAR(20)), N'Otávio Bastos Meireles', CAST(NULL AS NVARCHAR(20)), -234),
    (N'Logística', CAST(N'SEED-A041' AS NVARCHAR(20)), N'Cláudia Regina Vasconcelos', CAST(N'11943397603' AS NVARCHAR(20)), -292),
    (N'Logística', CAST(N'SEED-A042' AS NVARCHAR(20)), N'Henrique Damasceno Reis', CAST(N'11923401481' AS NVARCHAR(20)), -197),
    (N'Logística', CAST(N'SEED-A043' AS NVARCHAR(20)), N'Michele Torres Quintana', CAST(NULL AS NVARCHAR(20)), -319),
    (N'Logística', CAST(N'SEED-A044' AS NVARCHAR(20)), N'Sandro Luiz Peixoto', CAST(N'11964776133' AS NVARCHAR(20)), -127),
    (N'Logística', CAST(N'SEED-A045' AS NVARCHAR(20)), N'Elaine Cristina Bandeira', CAST(N'11950092516' AS NVARCHAR(20)), -110)
    ) AS v(NomeCurso, Matricula, Nome, Celular, Dias)
JOIN @cursos c ON c.Nome = v.NomeCurso;

/* ---------------------------------------------------------------------------
   6) Inscrições (AlunoModulo)
   O módulo é resolvido pelo curso do próprio aluno + número do módulo, então
   ninguém acaba inscrito em módulo de outro curso.
   StatusInscricao: 0=Aprovado, 1=Reprovado, 2=EmAndamento, 3=Cancelado.
   --------------------------------------------------------------------------- */
INSERT INTO AlunoModulo
    (IdAluno, IdModulo, DataAcesso, DataConclusao, DataMatricula, Nota,
     ObsTempo, ObsNota, ObsGeral, StatusInscricao, DataCadastro, Status)
SELECT a.IdAluno, m.IdModulo,
       DATEADD(DAY, v.DiasAcesso, @hoje),
       DATEADD(DAY, v.DiasFim, @hoje),
       DATEADD(DAY, v.DiasMatricula, @hoje),
       v.Nota,
       NULL,
       v.ObsNota,
       v.ObsGeral,
       v.StatusInscricao,
       DATEADD(DAY, v.DiasAcesso, @hoje),
       1
FROM (VALUES
    (CAST(N'Ana Maria Silva Rodrigues' AS NVARCHAR(100)), 1, -427, -381, -438, CAST(9.60 AS DECIMAL(4,2)), CAST(NULL AS NVARCHAR(MAX)), CAST(NULL AS NVARCHAR(MAX)), 0),
    (N'Ana Maria Silva Rodrigues', 2, -366, -320, NULL, 7.30, NULL, NULL, 0),
    (N'Ana Maria Silva Rodrigues', 3, -316, -280, -320, 9.00, NULL, NULL, 0),
    (N'Ana Maria Silva Rodrigues', 4, -275, -225, NULL, 7.60, NULL, NULL, 0),
    (N'Ana Maria Silva Rodrigues', 5, -214, -165, NULL, 9.90, NULL, NULL, 0),
    (N'Ana Maria Silva Rodrigues', 6, -162, -114, -165, 7.20, NULL, NULL, 0),
    (N'Ana Maria Silva Rodrigues', 7, -109, NULL, -112, 0.00, NULL, NULL, 2),
    (N'Carlos Eduardo Nascimento', 1, -97, -55, -103, 7.90, NULL, N'Transferido de turma.', 0),
    (N'Carlos Eduardo Nascimento', 2, -45, NULL, -48, 0.00, NULL, NULL, 2),
    (N'Beatriz Antunes Moreira', 1, -143, -101, NULL, 8.30, NULL, NULL, 0),
    (N'Beatriz Antunes Moreira', 2, -91, -59, -98, 9.60, NULL, NULL, 0),
    (N'Beatriz Antunes Moreira', 3, -52, NULL, -58, 0.00, NULL, NULL, 2),
    (N'João Pedro de Almeida Costa', 1, -587, -546, NULL, 9.10, NULL, NULL, 0),
    (N'João Pedro de Almeida Costa', 2, -536, -504, -542, 7.70, NULL, N'Aluno solicitou material complementar.', 0),
    (N'João Pedro de Almeida Costa', 3, -494, -455, -505, 8.20, NULL, NULL, 0),
    (N'João Pedro de Almeida Costa', 4, -449, -418, NULL, 9.50, NULL, NULL, 0),
    (N'João Pedro de Almeida Costa', 5, -409, -360, -418, 7.10, NULL, NULL, 0),
    (N'João Pedro de Almeida Costa', 6, -353, -320, -359, 7.40, NULL, NULL, 0),
    (N'João Pedro de Almeida Costa', 7, -308, -267, -319, 7.40, NULL, NULL, 0),
    (N'João Pedro de Almeida Costa', 8, -261, -229, -269, 9.80, NULL, NULL, 0),
    (N'João Pedro de Almeida Costa', 9, -225, -191, -232, 8.90, NULL, NULL, 0),
    (N'João Pedro de Almeida Costa', 10, -188, -150, -199, 8.60, NULL, N'Transferido de turma.', 0),
    (N'João Pedro de Almeida Costa', 11, -145, NULL, -149, 0.00, NULL, NULL, 2),
    (N'Mariana Souza Lima', 1, -485, -449, -495, 8.00, NULL, NULL, 0),
    (N'Mariana Souza Lima', 2, -443, -413, -450, 9.10, NULL, N'Aluno solicitou material complementar.', 0),
    (N'Mariana Souza Lima', 3, -402, -354, -410, 7.80, NULL, NULL, 0),
    (N'Mariana Souza Lima', 4, -350, -303, -360, 9.40, NULL, NULL, 0),
    (N'Mariana Souza Lima', 5, -291, -256, -300, 7.50, NULL, NULL, 0),
    (N'Mariana Souza Lima', 6, -244, -201, -247, 8.60, NULL, NULL, 0),
    (N'Mariana Souza Lima', 7, -189, -154, -199, 8.90, NULL, NULL, 0),
    (N'Mariana Souza Lima', 8, -141, -109, NULL, 7.80, NULL, NULL, 0),
    (N'Mariana Souza Lima', 9, -105, -65, NULL, 9.20, NULL, NULL, 0),
    (N'Mariana Souza Lima', 10, -61, NULL, -73, 0.00, NULL, NULL, 2),
    (N'Rafael Oliveira Duarte', 1, -494, -462, -498, 8.80, NULL, NULL, 0),
    (N'Rafael Oliveira Duarte', 2, -454, -420, NULL, 8.60, NULL, NULL, 0),
    (N'Rafael Oliveira Duarte', 3, -415, -375, -417, 7.00, NULL, NULL, 0),
    (N'Rafael Oliveira Duarte', 4, -371, -324, -375, 9.30, NULL, NULL, 0),
    (N'Rafael Oliveira Duarte', 5, -318, -286, -329, 8.00, NULL, NULL, 0),
    (N'Rafael Oliveira Duarte', 6, -276, -240, -280, 8.60, NULL, NULL, 0),
    (N'Rafael Oliveira Duarte', 7, -236, -197, NULL, 9.00, NULL, NULL, 0),
    (N'Rafael Oliveira Duarte', 8, -186, -142, -198, 9.40, NULL, NULL, 0),
    (N'Rafael Oliveira Duarte', 9, -133, NULL, -139, 0.00, NULL, NULL, 2),
    (N'Camila Fernandes Rocha', 1, -522, -479, -532, 9.80, NULL, NULL, 0),
    (N'Camila Fernandes Rocha', 2, -469, -429, NULL, 8.10, NULL, NULL, 0),
    (N'Camila Fernandes Rocha', 3, -414, -369, -421, 7.30, NULL, N'Retorno após afastamento.', 0),
    (N'Camila Fernandes Rocha', 4, -359, -325, -366, 7.20, NULL, NULL, 0),
    (N'Camila Fernandes Rocha', 5, -310, -263, NULL, 8.60, NULL, NULL, 0),
    (N'Camila Fernandes Rocha', 6, -248, -214, -252, 9.60, NULL, NULL, 0),
    (N'Camila Fernandes Rocha', 7, -206, -156, -218, 7.90, NULL, NULL, 0),
    (N'Camila Fernandes Rocha', 8, -142, -109, NULL, 9.90, NULL, NULL, 0),
    (N'Camila Fernandes Rocha', 9, -100, -63, -112, 7.30, NULL, NULL, 0),
    (N'Camila Fernandes Rocha', 10, -54, -11, NULL, 8.50, NULL, NULL, 0),
    (N'Camila Fernandes Rocha', 11, -1, NULL, -11, 0.00, NULL, NULL, 2),
    (N'Bruno Henrique Martins', 1, -607, -559, -618, 9.70, NULL, NULL, 0),
    (N'Bruno Henrique Martins', 2, -552, -506, -563, 8.80, NULL, NULL, 0),
    (N'Bruno Henrique Martins', 3, -491, -457, NULL, 7.30, NULL, NULL, 0),
    (N'Bruno Henrique Martins', 4, -449, -415, -458, 7.90, NULL, NULL, 0),
    (N'Bruno Henrique Martins', 5, -412, -374, NULL, 8.90, NULL, NULL, 0),
    (N'Bruno Henrique Martins', 6, -362, -314, -367, 7.10, NULL, NULL, 0),
    (N'Bruno Henrique Martins', 7, -301, -251, -306, 8.20, NULL, NULL, 0),
    (N'Bruno Henrique Martins', 8, -244, -206, -247, 7.10, NULL, N'Aluno solicitou material complementar.', 0),
    (N'Bruno Henrique Martins', 9, -193, -149, -202, 9.70, NULL, NULL, 0),
    (N'Bruno Henrique Martins', 10, -140, -90, -145, 9.50, NULL, NULL, 0),
    (N'Bruno Henrique Martins', 11, -85, -48, -93, 7.80, NULL, NULL, 0),
    (N'Bruno Henrique Martins', 12, -33, -1, -43, 9.60, NULL, NULL, 0),
    (N'Larissa Gomes Pereira', 1, -637, -601, NULL, 8.00, NULL, NULL, 0),
    (N'Larissa Gomes Pereira', 2, -598, -566, NULL, 10.00, NULL, NULL, 0),
    (N'Larissa Gomes Pereira', 3, -551, -505, -557, 10.00, NULL, NULL, 0),
    (N'Larissa Gomes Pereira', 4, -499, -451, NULL, 8.50, NULL, NULL, 0),
    (N'Larissa Gomes Pereira', 5, -448, -401, -456, 8.70, NULL, NULL, 0),
    (N'Larissa Gomes Pereira', 6, -397, -359, NULL, 7.50, NULL, NULL, 0),
    (N'Larissa Gomes Pereira', 7, -353, -317, -359, 9.10, NULL, NULL, 0),
    (N'Larissa Gomes Pereira', 8, -305, -260, -314, 9.00, NULL, NULL, 0),
    (N'Larissa Gomes Pereira', 9, -249, -210, -251, 8.40, NULL, N'Transferido de turma.', 0),
    (N'Larissa Gomes Pereira', 10, -202, -169, NULL, 9.40, NULL, NULL, 0),
    (N'Larissa Gomes Pereira', 11, -160, -121, NULL, 9.50, NULL, NULL, 0),
    (N'Larissa Gomes Pereira', 12, -112, -81, NULL, 8.70, NULL, N'Turma noturna.', 0),
    (N'Thiago Ribeiro Cardoso', 1, -627, -595, -638, 8.80, NULL, NULL, 0),
    (N'Thiago Ribeiro Cardoso', 2, -591, -558, NULL, 9.60, NULL, NULL, 0),
    (N'Thiago Ribeiro Cardoso', 3, -544, -506, -555, 9.40, NULL, N'Aluno solicitou material complementar.', 0),
    (N'Thiago Ribeiro Cardoso', 4, -496, -457, -499, 7.20, NULL, NULL, 0),
    (N'Thiago Ribeiro Cardoso', 5, -452, -411, -458, 8.70, NULL, N'Aluno solicitou material complementar.', 0),
    (N'Thiago Ribeiro Cardoso', 6, -399, -353, -409, 8.20, NULL, NULL, 0),
    (N'Thiago Ribeiro Cardoso', 7, -343, -304, -347, 7.00, NULL, NULL, 0),
    (N'Thiago Ribeiro Cardoso', 8, -290, -241, -294, 8.40, NULL, NULL, 0),
    (N'Thiago Ribeiro Cardoso', 9, -238, -189, -245, 7.30, NULL, NULL, 0),
    (N'Thiago Ribeiro Cardoso', 10, -177, -128, NULL, 7.00, NULL, NULL, 0),
    (N'Thiago Ribeiro Cardoso', 11, -125, -75, -129, 8.40, NULL, NULL, 0),
    (N'Thiago Ribeiro Cardoso', 12, -71, -26, NULL, 9.50, NULL, NULL, 0),
    (N'Juliana Barbosa Freitas', 1, -263, -225, -271, 7.80, NULL, NULL, 0),
    (N'Juliana Barbosa Freitas', 2, -214, -171, -222, 7.80, NULL, NULL, 0),
    (N'Juliana Barbosa Freitas', 3, -168, -118, -170, 9.90, NULL, NULL, 0),
    (N'Juliana Barbosa Freitas', 4, -110, -67, -117, 8.40, NULL, NULL, 0),
    (N'Felipe Augusto Ramos', 1, -469, -427, NULL, 10.00, NULL, NULL, 0),
    (N'Felipe Augusto Ramos', 2, -414, -367, NULL, 9.20, NULL, NULL, 0),
    (N'Felipe Augusto Ramos', 3, -358, -321, NULL, 9.90, NULL, NULL, 0),
    (N'Felipe Augusto Ramos', 4, -307, -265, -309, 7.90, NULL, NULL, 0),
    (N'Felipe Augusto Ramos', 5, -262, -218, -267, 8.80, NULL, NULL, 0),
    (N'Felipe Augusto Ramos', 6, -203, -166, -210, 8.20, NULL, NULL, 0),
    (N'Felipe Augusto Ramos', 7, -160, -117, NULL, 9.20, NULL, NULL, 0),
    (N'Felipe Augusto Ramos', 8, -103, -56, NULL, 9.00, NULL, N'Turma noturna.', 0),
    (N'Patrícia Nunes Teixeira', 1, -335, -299, NULL, 8.70, NULL, NULL, 0),
    (N'Patrícia Nunes Teixeira', 2, -292, -259, NULL, 8.70, NULL, N'Aluno solicitou material complementar.', 0),
    (N'Patrícia Nunes Teixeira', 3, -247, -202, -256, 7.30, NULL, NULL, 0),
    (N'Patrícia Nunes Teixeira', 4, -197, -150, -207, 8.80, NULL, NULL, 0),
    (N'Patrícia Nunes Teixeira', 5, -138, -88, -141, 7.70, NULL, NULL, 0),
    (N'Patrícia Nunes Teixeira', 6, -73, -24, -75, 8.90, NULL, NULL, 0),
    (N'Gustavo Henrique Lopes', 1, -557, -507, -563, 9.40, NULL, NULL, 0),
    (N'Gustavo Henrique Lopes', 2, -503, -470, -510, 9.70, NULL, NULL, 0),
    (N'Gustavo Henrique Lopes', 3, -464, -432, -475, 7.50, NULL, NULL, 0),
    (N'Gustavo Henrique Lopes', 4, -426, -386, NULL, 7.40, NULL, NULL, 0),
    (N'Gustavo Henrique Lopes', 5, -371, -341, -378, 4.40, N'Avaliação substitutiva aplicada.', NULL, 1),
    (N'Gustavo Henrique Lopes', 6, -330, -285, -340, 9.30, NULL, NULL, 0),
    (N'Gustavo Henrique Lopes', 7, -279, -235, -289, 8.80, NULL, NULL, 0),
    (N'Gustavo Henrique Lopes', 8, -228, -178, NULL, 7.70, NULL, NULL, 0),
    (N'Gustavo Henrique Lopes', 9, -174, -136, NULL, 9.90, NULL, NULL, 0),
    (N'Gustavo Henrique Lopes', 10, -133, -101, -144, 5.40, NULL, NULL, 1),
    (N'Gustavo Henrique Lopes', 11, -88, -44, -97, 9.90, NULL, NULL, 0),
    (N'Gustavo Henrique Lopes', 12, -38, NULL, -42, 0.00, NULL, NULL, 2),
    (N'Amanda Cristina Vieira', 1, -438, -396, NULL, 6.00, N'Avaliação substitutiva aplicada.', NULL, 1),
    (N'Amanda Cristina Vieira', 2, -381, -346, NULL, 7.20, NULL, NULL, 0),
    (N'Amanda Cristina Vieira', 3, -336, -290, -347, 8.80, NULL, NULL, 0),
    (N'Amanda Cristina Vieira', 4, -285, -249, NULL, 7.10, NULL, NULL, 0),
    (N'Amanda Cristina Vieira', 5, -243, -213, -252, 9.80, NULL, NULL, 0),
    (N'Amanda Cristina Vieira', 6, -202, -153, -204, 7.30, NULL, NULL, 0),
    (N'Amanda Cristina Vieira', 7, -142, -97, NULL, 8.40, NULL, NULL, 0),
    (N'Amanda Cristina Vieira', 8, -89, -40, -96, 3.20, N'Segunda tentativa.', NULL, 1),
    (N'Amanda Cristina Vieira', 9, -25, NULL, -36, 0.00, NULL, NULL, 2),
    (N'Rodrigo Menezes Barros', 1, -471, -439, NULL, 5.90, N'Segunda tentativa.', NULL, 1),
    (N'Rodrigo Menezes Barros', 2, -436, -399, -444, 4.70, NULL, NULL, 1),
    (N'Rodrigo Menezes Barros', 3, -390, -346, -393, 9.60, NULL, NULL, 0),
    (N'Rodrigo Menezes Barros', 4, -335, -303, NULL, 9.40, NULL, NULL, 0),
    (N'Rodrigo Menezes Barros', 5, -288, -244, -299, 9.10, NULL, NULL, 0),
    (N'Rodrigo Menezes Barros', 6, -231, -187, -240, 7.00, NULL, NULL, 0),
    (N'Rodrigo Menezes Barros', 7, -184, -139, NULL, 9.30, NULL, NULL, 0),
    (N'Rodrigo Menezes Barros', 8, -127, NULL, NULL, 0.00, NULL, NULL, 2),
    (N'Letícia Carvalho Pinto', 1, -522, -481, -528, 9.10, NULL, NULL, 0),
    (N'Letícia Carvalho Pinto', 2, -472, -441, -476, 9.70, NULL, NULL, 0),
    (N'Letícia Carvalho Pinto', 3, -437, -400, NULL, 8.40, NULL, NULL, 0),
    (N'Letícia Carvalho Pinto', 4, -395, -353, -406, 8.30, NULL, N'Transferido de turma.', 0),
    (N'Letícia Carvalho Pinto', 5, -340, -299, -350, 9.10, NULL, NULL, 0),
    (N'Letícia Carvalho Pinto', 6, -290, -244, -300, 9.90, NULL, NULL, 0),
    (N'Letícia Carvalho Pinto', 7, -230, -190, -239, 7.70, NULL, NULL, 0),
    (N'Letícia Carvalho Pinto', 8, -182, -132, -190, 7.20, NULL, NULL, 0),
    (N'Letícia Carvalho Pinto', 9, -43, -6, NULL, 0.00, NULL, N'Cancelamento solicitado pelo aluno.', 3),
    (N'Vinícius Moraes Cunha', 1, -557, -521, -565, 8.00, NULL, NULL, 0),
    (N'Vinícius Moraes Cunha', 2, -508, -460, NULL, 7.40, NULL, NULL, 0),
    (N'Vinícius Moraes Cunha', 3, -445, -405, -451, 7.90, NULL, NULL, 0),
    (N'Vinícius Moraes Cunha', 4, -397, -347, -400, 8.50, NULL, NULL, 0),
    (N'Vinícius Moraes Cunha', 5, -340, -297, -342, 7.50, NULL, NULL, 0),
    (N'Vinícius Moraes Cunha', 6, -289, -259, NULL, 7.40, NULL, NULL, 0),
    (N'Vinícius Moraes Cunha', 7, -256, -219, NULL, 8.00, NULL, N'Transferido de turma.', 0),
    (N'Vinícius Moraes Cunha', 8, -210, -163, -215, 9.20, NULL, NULL, 0),
    (N'Vinícius Moraes Cunha', 9, -159, -119, -170, 7.50, NULL, NULL, 0),
    (N'Vinícius Moraes Cunha', 10, -110, -76, NULL, 9.30, NULL, NULL, 0),
    (N'Vinícius Moraes Cunha', 11, -30, -2, -41, 0.00, NULL, N'Cancelamento solicitado pelo aluno.', 3),
    (N'Fernanda Alves Machado', 1, -289, -248, -299, 7.10, NULL, NULL, 0),
    (N'Fernanda Alves Machado', 2, -238, -204, NULL, 8.20, NULL, N'Turma noturna.', 0),
    (N'Fernanda Alves Machado', 3, -195, -157, -197, 9.40, NULL, NULL, 0),
    (N'Fernanda Alves Machado', 4, -150, -117, -157, 7.40, NULL, NULL, 0),
    (N'Fernanda Alves Machado', 5, -109, -64, NULL, 7.70, NULL, NULL, 0),
    (N'Fernanda Alves Machado', 6, -60, NULL, NULL, 0.00, NULL, NULL, 2),
    (N'Marcelo Tadeu Siqueira', 1, -204, -163, NULL, 7.30, NULL, NULL, 0),
    (N'Marcelo Tadeu Siqueira', 2, -157, NULL, -163, 0.00, NULL, NULL, 2),
    (N'Isabela Monteiro Dias', 1, -289, -244, -296, 7.60, NULL, NULL, 0),
    (N'Isabela Monteiro Dias', 2, -230, -189, -233, 8.30, NULL, NULL, 0),
    (N'Isabela Monteiro Dias', 3, -186, -138, -197, 9.90, NULL, NULL, 0),
    (N'Isabela Monteiro Dias', 4, -134, NULL, -137, 0.00, NULL, NULL, 2),
    (N'Leonardo Farias Correia', 1, -385, -342, -390, 7.40, NULL, NULL, 0),
    (N'Leonardo Farias Correia', 2, -328, -283, -336, 7.80, NULL, NULL, 0),
    (N'Leonardo Farias Correia', 3, -275, -229, -287, 7.60, NULL, NULL, 0),
    (N'Leonardo Farias Correia', 4, -217, -173, NULL, 7.60, NULL, NULL, 0),
    (N'Leonardo Farias Correia', 5, -165, -126, -174, 8.40, NULL, NULL, 0),
    (N'Leonardo Farias Correia', 6, -119, NULL, -130, 0.00, NULL, NULL, 2),
    (N'Natália Prado Bezerra', 1, -209, -172, NULL, 7.40, NULL, NULL, 0),
    (N'Natália Prado Bezerra', 2, -53, -4, NULL, 0.00, NULL, N'Cancelamento solicitado pelo aluno.', 3),
    (N'Diego Antunes Cavalcanti', 1, -311, -280, -322, 10.00, NULL, NULL, 0),
    (N'Diego Antunes Cavalcanti', 2, -270, -239, -281, 7.90, NULL, NULL, 0),
    (N'Diego Antunes Cavalcanti', 3, -231, -191, -237, 8.50, NULL, NULL, 0),
    (N'Diego Antunes Cavalcanti', 4, -182, -150, -188, 9.20, NULL, NULL, 0),
    (N'Diego Antunes Cavalcanti', 5, -69, -10, NULL, 0.00, NULL, N'Cancelamento solicitado pelo aluno.', 3),
    (N'Priscila Ramos Andrade', 1, -143, -109, -146, 8.70, NULL, NULL, 0),
    (N'Priscila Ramos Andrade', 2, -98, -67, -100, 9.40, NULL, NULL, 0),
    (N'Priscila Ramos Andrade', 3, -36, -7, -38, 0.00, NULL, N'Cancelamento solicitado pelo aluno.', 3),
    (N'André Luiz Sampaio', 1, -187, -139, -196, 8.20, NULL, NULL, 0),
    (N'André Luiz Sampaio', 2, -136, -88, -143, 7.20, NULL, NULL, 0),
    (N'André Luiz Sampaio', 3, -71, -12, -76, 0.00, NULL, N'Cancelamento solicitado pelo aluno.', 3),
    (N'Vanessa Coelho Braga', 1, -187, -154, -189, 8.70, NULL, NULL, 0),
    (N'Vanessa Coelho Braga', 2, -144, -104, NULL, 9.50, NULL, NULL, 0),
    (N'Vanessa Coelho Braga', 3, -99, -60, -111, 7.70, NULL, NULL, 0),
    (N'Eduardo Pacheco Nogueira', 1, -158, -126, NULL, 7.40, NULL, NULL, 0),
    (N'Aline Ferreira Batista', 1, -299, -255, -305, 9.60, NULL, NULL, 0),
    (N'Aline Ferreira Batista', 2, -250, -211, -256, 9.20, NULL, NULL, 0),
    (N'Aline Ferreira Batista', 3, -202, -172, -209, 7.40, NULL, NULL, 0),
    (N'Aline Ferreira Batista', 4, -168, -130, -179, 9.60, NULL, N'Retorno após afastamento.', 0),
    (N'Aline Ferreira Batista', 5, -115, -81, NULL, 8.10, NULL, NULL, 0),
    (N'Aline Ferreira Batista', 6, -68, -29, -77, 8.30, NULL, NULL, 0),
    (N'Ricardo Salles Tavares', 1, -152, -117, -156, 3.40, NULL, NULL, 1),
    (N'Ricardo Salles Tavares', 2, -103, -58, NULL, 3.60, NULL, NULL, 1),
    (N'Ricardo Salles Tavares', 3, -46, NULL, -52, 0.00, NULL, NULL, 2),
    (N'Tatiane Miranda Rezende', 1, -240, -197, -244, 2.50, N'Nota lançada após recuperação.', N'Retorno após afastamento.', 1),
    (N'Tatiane Miranda Rezende', 2, -193, -159, -198, 4.80, N'Segunda tentativa.', NULL, 1),
    (N'Tatiane Miranda Rezende', 3, -150, NULL, -161, 0.00, NULL, NULL, 2),
    (N'Paulo Sérgio Fonseca', 1, -240, -198, -246, 5.50, NULL, NULL, 1),
    (N'Paulo Sérgio Fonseca', 2, -195, -164, -206, 2.80, N'Segunda tentativa.', NULL, 1),
    (N'Paulo Sérgio Fonseca', 3, -157, NULL, NULL, 0.00, NULL, NULL, 2),
    (N'Débora Cristina Aguiar', 1, -160, -127, -172, 6.10, NULL, NULL, 1),
    (N'Débora Cristina Aguiar', 2, -116, -75, -124, 6.10, N'Avaliação substitutiva aplicada.', NULL, 1),
    (N'Débora Cristina Aguiar', 3, -68, NULL, NULL, 0.00, NULL, NULL, 2),
    (N'Wesley Cardoso Pontes', 1, -261, -213, NULL, 7.90, NULL, NULL, 0),
    (N'Wesley Cardoso Pontes', 2, -203, -173, -211, 9.90, NULL, NULL, 0),
    (N'Wesley Cardoso Pontes', 3, -163, -119, -173, 4.50, N'Segunda tentativa.', NULL, 1),
    (N'Wesley Cardoso Pontes', 4, -107, -75, -119, 4.60, NULL, N'Aluno solicitou material complementar.', 1),
    (N'Wesley Cardoso Pontes', 5, -69, NULL, -79, 0.00, NULL, NULL, 2),
    (N'Renata Aparecida Lima', 1, -229, -183, -235, 8.60, NULL, NULL, 0),
    (N'Renata Aparecida Lima', 2, -170, -129, -175, 7.30, NULL, N'Retorno após afastamento.', 0),
    (N'Renata Aparecida Lima', 3, -114, -75, -120, 9.40, NULL, NULL, 0),
    (N'Renata Aparecida Lima', 4, -70, NULL, -78, 0.00, NULL, NULL, 2),
    (N'Fábio Junqueira Neves', 1, -252, -218, NULL, 9.70, NULL, NULL, 0),
    (N'Fábio Junqueira Neves', 2, -209, -161, -217, 9.40, NULL, NULL, 0),
    (N'Fábio Junqueira Neves', 3, -153, NULL, NULL, 0.00, NULL, NULL, 2),
    (N'Simone Rocha Guimarães', 1, -247, -205, NULL, 7.90, NULL, NULL, 0),
    (N'Simone Rocha Guimarães', 2, -195, -146, NULL, 7.60, NULL, NULL, 0),
    (N'Simone Rocha Guimarães', 3, -142, -94, NULL, 7.70, NULL, NULL, 0),
    (N'Simone Rocha Guimarães', 4, -91, -49, NULL, 9.30, NULL, NULL, 0),
    (N'Simone Rocha Guimarães', 5, -42, NULL, NULL, 0.00, NULL, NULL, 2),
    (N'Alexandre Pires Domingues', 1, -265, -235, -267, 8.90, NULL, NULL, 0),
    (N'Alexandre Pires Domingues', 2, -222, -178, -230, 8.20, NULL, NULL, 0),
    (N'Alexandre Pires Domingues', 3, -173, -134, -178, 7.80, NULL, NULL, 0),
    (N'Alexandre Pires Domingues', 4, -123, -83, -130, 9.60, NULL, NULL, 0),
    (N'Alexandre Pires Domingues', 5, -72, -42, -81, 8.10, NULL, NULL, 0),
    (N'Bianca Toledo Amaral', 1, -132, -87, -142, 9.20, NULL, NULL, 0),
    (N'Otávio Bastos Meireles', 1, -135, -91, -147, 8.10, NULL, NULL, 0),
    (N'Otávio Bastos Meireles', 2, -86, NULL, -92, 0.00, NULL, NULL, 2),
    (N'Cláudia Regina Vasconcelos', 1, -146, -106, NULL, 9.00, NULL, N'Retorno após afastamento.', 0),
    (N'Cláudia Regina Vasconcelos', 2, -98, NULL, -101, 0.00, NULL, NULL, 2),
    (N'Henrique Damasceno Reis', 1, -241, -203, -246, 8.00, NULL, NULL, 0),
    (N'Henrique Damasceno Reis', 2, -200, -159, -206, 8.10, NULL, N'Turma noturna.', 0),
    (N'Henrique Damasceno Reis', 3, -145, -105, -153, 9.70, NULL, NULL, 0),
    (N'Michele Torres Quintana', 1, -143, -103, NULL, 9.10, NULL, NULL, 0),
    (N'Michele Torres Quintana', 2, -94, -49, NULL, 9.50, NULL, NULL, 0),
    (N'Michele Torres Quintana', 3, -37, -1, -46, 7.40, NULL, NULL, 0),
    (N'Sandro Luiz Peixoto', 1, -146, -109, -150, 7.50, NULL, N'Retorno após afastamento.', 0),
    (N'Elaine Cristina Bandeira', 1, -229, -180, -233, 9.00, NULL, NULL, 0),
    (N'Elaine Cristina Bandeira', 2, -174, -141, NULL, 8.70, NULL, NULL, 0),
    (N'Elaine Cristina Bandeira', 3, -50, -23, -59, 0.00, NULL, N'Cancelamento solicitado pelo aluno.', 3)
    ) AS v(NomeAluno, NumeroModulo, DiasAcesso, DiasFim, DiasMatricula, Nota,
            ObsNota, ObsGeral, StatusInscricao)
JOIN @alunos a  ON a.Nome = v.NomeAluno
JOIN @modulos m ON m.IdCurso = a.IdCurso AND m.Numero = v.NumeroModulo;

/* ---------------------------------------------------------------------------
   7) Conferência
   --------------------------------------------------------------------------- */
SELECT 'Professores' AS Tabela, COUNT(*) AS Registros FROM @professores
UNION ALL SELECT 'Cursos',      COUNT(*) FROM @cursos
UNION ALL SELECT 'Modulos',     COUNT(*) FROM @modulos
UNION ALL SELECT 'Alunos',      COUNT(*) FROM @alunos
UNION ALL SELECT 'Inscricoes',  COUNT(*) FROM AlunoModulo
          WHERE IdAluno IN (SELECT IdAluno FROM @alunos);
