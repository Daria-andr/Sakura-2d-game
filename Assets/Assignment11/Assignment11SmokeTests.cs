#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Assignment11SmokeTests : MonoBehaviour
{
    string report=""; int count;
    void Check(bool ok,string message) { if(!ok) throw new Exception(message); report+="PASS "+(++count)+": "+message+"\n"; File.WriteAllText("Logs/Assignment11-tests.txt",report); }
    IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        var routine=Run();
        while(true) {
            object next;
            try { if(!routine.MoveNext()) break; next=routine.Current; }
            catch(Exception e) { File.WriteAllText("Logs/Assignment11-tests.txt",report+"FAIL: "+e); Finish(1); yield break; }
            yield return next;
        }
        File.WriteAllText("Logs/Assignment11-tests.txt",report+"Verified "+count+" assertions in Unity Play Mode.\n");
        Finish(0);
    }
    static void Finish(int code)
    {
        if (Application.isBatchMode) UnityEditor.EditorApplication.Exit(code);
        else UnityEditor.EditorApplication.isPlaying = false;
    }
    IEnumerator Run()
    {
        SaveSystem.TestPath=Path.Combine(Application.persistentDataPath,"Assignment11Tests","progress.json");
        if(File.Exists(SaveSystem.FilePath)) File.Delete(SaveSystem.FilePath);
        SaveSystem.Reload();
        Check(SaveSystem.Data.highestUnlocked==0 && File.Exists(SaveSystem.FilePath),"Missing file creates persisted defaults");
        foreach(string bad in new[]{"", "{", "null", "{}", "{\"version\":99}", "{\"version\":1,\"highestUnlocked\":8,\"currentLevel\":0,\"completed\":[false,false,false],\"bestScores\":[0,0,0],\"volume\":1}"}) {
            File.WriteAllText(SaveSystem.FilePath,bad);SaveSystem.Reload();
            Check(SaveSystem.Valid(SaveSystem.Data) && SaveSystem.Data.highestUnlocked==0,"Invalid save recovers: "+bad);
        }
        var invalid=SaveSystem.Defaults();invalid.volume=float.NaN;
        Check(!SaveSystem.Valid(invalid),"Non-finite volume rejected");
        invalid=SaveSystem.Defaults();invalid.completed=new[]{false,true,false};
        Check(!SaveSystem.Valid(invalid),"Inconsistent completion rejected");
        invalid=SaveSystem.Defaults();invalid.bestScores[0]=-1;
        Check(!SaveSystem.Valid(invalid),"Negative scores rejected");
        Check(!SaveSystem.PlayLevel(2),"Locked level cannot be launched");
        var ui=FindAnyObjectByType<ProgressMenu>();
        Check(ui!=null && ui.levelButtons.Length==3,"Saved menu scene has three wired level buttons");
        ui.OpenSettings();ui.volumeSlider.onValueChanged.Invoke(.4f);
        Check(JsonUtility.FromJson<ProgressData>(File.ReadAllText(SaveSystem.FilePath)).volume==.4f,"Settings slider automatically writes JSON");
        ui.RequestReset();Check(ui.confirmPanel.activeSelf,"Reset confirmation visible");
        string before=File.ReadAllText(SaveSystem.FilePath);
        ui.CancelReset();Check(!ui.confirmPanel.activeSelf && before==File.ReadAllText(SaveSystem.FilePath),"Cancel preserves file unchanged");
        ui.RequestReset();ui.ConfirmReset();
        Check(SaveSystem.Data.volume==1 && !ui.levelButtons[1].interactable,"Confirm resets data and UI immediately");
        Check(JsonUtility.FromJson<ProgressData>(File.ReadAllText(SaveSystem.FilePath)).volume==1,"Reset persisted");
        ui.CloseSettings();

        for(int level=0;level<3;level++) {
            Check(SaveSystem.PlayLevel(level),"Unlocked level launches: "+level);
            yield return null; yield return null;
            var context=FindAnyObjectByType<LevelContext>();
            var data=context.levelData;
            Check(data!=null && data.levelIndex==level && data==LevelContext.Current,"Correct LevelData connected: "+level);
            var hero=FindAnyObjectByType<PlayerMovement>();
            Check(hero!=null && hero.moveSpeed==data.playerMovement.moveSpeed && hero.jumpForce>0,"Player reads level data: "+level);
            var health=hero.GetComponent<PlayerHealth>();
            Check(health.maxHealth==data.playerHealth.maxHealth && health.maxHealth>0,"Health reads level data");
            foreach(var enemy in FindObjectsByType<Enemy>()) Check(enemy.maxHealth==data.enemy.maxHealth && enemy.moveSpeed==data.enemy.moveSpeed,"Enemy reads level data");
            var gm=GameManager.Instance;
            SaveSystem.Data.volume=.61f;gm.SetPaused(true);
            Check(Time.timeScale==0 && JsonUtility.FromJson<ProgressData>(File.ReadAllText(SaveSystem.FilePath)).volume==.61f,"Pause saves and pauses gameplay");
            gm.SetPaused(false);Check(Time.timeScale==1,"Resume restores gameplay");
            SaveSystem.Data.volume=.62f;context.SendMessage("OnApplicationFocus",false);
            Check(JsonUtility.FromJson<ProgressData>(File.ReadAllText(SaveSystem.FilePath)).volume==.62f,"Focus loss saves");
            SaveSystem.Data.volume=.63f;context.SendMessage("OnApplicationPause",true);
            Check(JsonUtility.FromJson<ProgressData>(File.ReadAllText(SaveSystem.FilePath)).volume==.63f,"Application pause saves");
            if(level==0) {
                float original=data.playerMovement.moveSpeed;
                data.playerMovement.moveSpeed=original+.5f;
                var probe=new GameObject("Data probe");probe.SetActive(false);probe.AddComponent<Rigidbody2D>();var motion=probe.AddComponent<PlayerMovement>();probe.SetActive(true);
                Check(motion.moveSpeed==original+.5f,"Changing LevelData changes newly initialized gameplay");
                data.playerMovement.moveSpeed=original;Destroy(probe);
                var shot=Instantiate(hero.bulletPrefab);var bullet=shot.GetComponent<Bullet>();
                Check(bullet.damage==data.bullet.damage && bullet.speed==data.bullet.speed,"Spawned bullet reads LevelData");Destroy(shot);
                hero.SetMobileMove(1);float x=hero.transform.position.x;
                yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
                Check(hero.transform.position.x>x,"Existing mobile movement moves the player");hero.ClearInput();
            }
            foreach(var enemy in FindObjectsByType<Enemy>()) enemy.TakeDamage(enemy.maxHealth);
            if(gm.boss!=null) {
                Check(gm.boss.CurrentHealth==data.bossController.maxHealth,"Boss health reads LevelData");
                gm.boss.TakeDamage(gm.boss.CurrentHealth);
                yield return new WaitForSecondsRealtime(data.bossController.deathAnimationDuration+.2f);
            } else {
                Check(gm.portal!=null && gm.portal.isOpen,"Defeating existing enemies opens portal");
                gm.portal.SendMessage("OnTriggerEnter2D",hero.GetComponent<Collider2D>());
            }
            Check(SaveSystem.Data.completed[level] && Time.timeScale==0,"Existing victory flow saves completion: "+level);
            var disk=JsonUtility.FromJson<ProgressData>(File.ReadAllText(SaveSystem.FilePath));
            Check(disk.completed[level] && disk.bestScores[level]>0,"Completion and score written to JSON");
        }
        SaveSystem.Reload();Check(SaveSystem.Data.completed.All(c=>c),"All three completions reload");
        SaveSystem.ResetProgress();Check(SaveSystem.Data.highestUnlocked==0 && SaveSystem.Data.bestScores.All(s=>s==0),"Reset clears unlocked levels and best scores");
        SaveSystem.Complete(0,700);SaveSystem.SetVolume(.37f);
        Check(SaveSystem.Save(),"Persist cross-process restart fixture");
    }
}
#endif

