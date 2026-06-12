using Microsoft.EntityFrameworkCore;

namespace Persistence.Seeders {
  public class StoredFunctionSeeder : ISeeder {
    public static FormattableString base36_encode = $@"
      CREATE OR REPLACE FUNCTION base36_encode(IN digits bigint, IN min_width int = 0) RETURNS varchar AS $$
      DECLARE
          chars char[]; 
          ret varchar; 
          val bigint; 
      BEGIN
          chars := ARRAY['0','1','2','3','4','5','6','7','8','9','a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'];
          val := digits; 
          ret := ''; 
          IF val < 0 THEN 
              val := val * -1; 
          END IF; 
          WHILE val != 0 LOOP 
              ret := chars[(val % 36)+1] || ret; 
              val := val / 36; 
          END LOOP;

          IF min_width > 0 AND char_length(ret) < min_width THEN 
              ret := lpad(ret, min_width, '0'); 
          END IF;

          RETURN ret;
      END;
      $$ LANGUAGE plpgsql IMMUTABLE;

      create or replace function create_child_cat(
          _parent_id ""Cats"".""Id""%type,
          child_id ""Cats"".""Id""%type
      ) returns ""Cats"" as $$
      declare
          my_left ""Cats"".""LeftEar""%type;
          child_cat ""Cats"";
      begin
          select ""LeftEar"" into my_left from ""Cats"" where ""Id"" = _parent_id;
          update ""Cats"" set ""RightEar"" = ""RightEar"" + 2 where ""RightEar"" > my_left;
          update ""Cats"" set ""LeftEar"" = ""LeftEar"" + 2 where ""LeftEar"" > my_left;
          insert into ""Cats""(""Id"", ""Name"", ""LeftEar"", ""RightEar"", ""ParentId"")
          values(child_id, '', my_left + 1, my_left + 2, _parent_id)
          returning * into child_cat;
          return child_cat;
      end;
      $$ language plpgsql;

      create or replace function create_sibling_cat(
          right_cat_id ""Cats"".""Id""%type,
          child_id ""Cats"".""Id""%type
      ) returns ""Cats"" as $$
      declare
          my_right ""Cats"";
          child_cat ""Cats"";
      begin
          select * into my_right from ""Cats"" where ""Id"" = right_cat_id;
          update ""Cats"" set ""RightEar"" = ""RightEar"" + 2 where ""RightEar"" > my_right.""RightEar"";
          update ""Cats"" set ""LeftEar"" = ""LeftEar"" + 2 where ""LeftEar"" > my_right.""RightEar"";
          insert into ""Cats""(""Id"", ""Name"", ""LeftEar"", ""RightEar"", ""ParentId"")
          values(child_id, '', my_right.""RightEar"" + 1, my_right.""RightEar"" + 2, my_right.""ParentId"")
          returning * into child_cat;
          return child_cat;
      end;
      $$ language plpgsql;

      create or replace function delete_leaf_cat(cat_id ""Cats"".""Id""%type) returns void as $$
      declare
          my_left ""Cats"".""LeftEar""%type;
          my_right ""Cats"".""RightEar""%type;
          my_width ""Cats"".""LeftEar""%type;
      begin
          select ""LeftEar"", ""RightEar"", ""RightEar"" - ""LeftEar"" + 1
              into my_left, my_right, my_width
          from ""Cats"" where ""Id"" = cat_id;
          delete from ""Cats"" where ""LeftEar"" between my_left and my_right;
          update ""Cats"" set ""RightEar"" = ""RightEar"" - my_width where ""RightEar"" > my_right;
          update ""Cats"" set ""LeftEar"" = ""LeftEar"" - my_width where ""LeftEar"" > my_right;
      end;
      $$ language plpgsql;
    ";
    public static async Task seed(AppDbContext context, IServiceProvider sp) {
      await context.Database.ExecuteSqlAsync(base36_encode);
    }
  }
}
